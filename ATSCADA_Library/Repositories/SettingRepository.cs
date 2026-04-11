using ATSCADA_Library.Data;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Interfaces.Server;
using ATSCADA_Library.Services.BackgroundServices;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Repositories
{

    public class SettingRepository : ISettingRepository
    {
        private readonly ATSCADADbContext _context;
        private readonly GetDataReportService _getDataReportService;

        public SettingRepository(GetDataReportService getDataReportService,ATSCADADbContext context)
        {
            _getDataReportService = getDataReportService;
            _context = context;
        }
        public async Task<Setting> GetAllSettingsAsync()
        {
            var record = await _context.Settings
            .ToListAsync();
            return record[0];
        }
        public async Task UpdateSettingAsync(long id, SettingUpdateRequest updateRequest)
        {
            var existingSetting = await _context.Settings.FindAsync(id);
            if (existingSetting == null)
            {
                throw new Exception("Setting not found.");
            }
            // Chỉ cập nhật những thuộc tính có giá trị (không phải null)
            if (updateRequest.IsActiveSendZNS.HasValue)
            {
                existingSetting.IsActiveSendZNS = updateRequest.IsActiveSendZNS.Value;
            }

            if (updateRequest.IsAppointmentForm.HasValue)
            {
                existingSetting.IsAppointmentForm = updateRequest.IsAppointmentForm.Value;
            }
            if (updateRequest.IsSpeechCall.HasValue)
            {
                existingSetting.IsActiveSpeechCall = updateRequest.IsSpeechCall.Value;
            }
            if (updateRequest.ScheduledTaskTime.HasValue)
            {
                existingSetting.ScheduledTaskTime = updateRequest.ScheduledTaskTime.Value;
            }
            if (updateRequest.FooterTextCountersMainView != null)
            {
                existingSetting.FooterTextCountersMainView = updateRequest.FooterTextCountersMainView;
            }
            if (updateRequest.FooterTextFontSize != 0)
            {
                existingSetting.FooterTextFontSize = updateRequest.FooterTextFontSize;
            }
            if (updateRequest.FooterTextColor != null)
            {
                existingSetting.FooterTextColor = updateRequest.FooterTextColor;
            }
            if (updateRequest.MaxVisibleCounters != 0)
            {
                existingSetting.MaxVisibleCounters = updateRequest.MaxVisibleCounters;
            }
            if (updateRequest.VideoUrl != null)
            {
                existingSetting.UrlVideoCountersMainView = updateRequest.VideoUrl;
            }
            if (updateRequest.ZnsTemplateName != null)
            {
                existingSetting.SendZnsWithTemplate = updateRequest.ZnsTemplateName;
            }
            _context.Settings.Update(existingSetting);
            await _context.SaveChangesAsync();
        }
        public async Task<int> UpdateZNSTimeNotificationAsync(int znsTimeNotification)
        {
            var setting = await _context.Settings.ToListAsync();
            //setting[0].ZNSTimeNotification = znsTimeNotification;
            _context.Settings.Update(setting[0]);
            await _context.SaveChangesAsync();
            //return setting[0].ZNSTimeNotification;
            return 0;
        }
        public async Task RunScheduledTaskNow(CancellationToken cancellationToken)
        {
            

            // Lấy thời điểm bắt đầu và kết thúc của ngày hôm qua
            var startTime = DateTime.Now.AddDays(-1);
            //var endTime = DateTime.Today.AddTicks(-1);
            var endTime = DateTime.Now;
            // Lấy dữ liệu từ Database
            var oldEntries = _context.Queues.Where(q => q.AppointmentDate >= startTime && q.AppointmentDate <= endTime).ToList();

            if (oldEntries.Any())
            {
                // Map old entries to QueueHistories
                var queueHistories = oldEntries.Select(q => new QueueHistory
                {
                    NameService = q.NameService,
                    OrderNumber = q.OrderNumber,
                    DescriptionService = q.DescriptionService,
                    FullName = q.FullName,
                    PhoneNumber = q.PhoneNumber,
                    AppointmentDate = q.AppointmentDate,
                    Message = q.Message,
                    Status = q.Status,
                    Verified = q.Verified,
                    ServiceId = q.ServiceId,
                    CounterId = q.CounterId,
                    OriginalCounterId = q.OriginalCounterId,
                    AppointmentId = q.AppointmentId,
                    PrintTime = q.PrintTime,
                    isNotified = q.isNotified,
                    Priority = q.Priority,
                    LastTimeUpdated = q.LastTimeUpdated
                }).ToList();

                // Lưu dữ liệu vào bảng History
                _context.QueueHistories.AddRange(queueHistories);

                // Xoá dữ liệu từ bảng Queue
                _context.Queues.RemoveRange(oldEntries);

                // Reset các counters về 0
                var allCounters = _context.Counters.Where(c => c.IsActive == true).ToList();
                foreach (var counter in allCounters)
                {
                    counter.CurrentNumber = 0;
                    counter.TotalCount = 0;
                }

                // Cập nhật thời gian thực thi cuối cùng
                var setting = _context.Settings.FirstOrDefault();
                if (setting != null)
                {
                    setting.LastTaskExecuted = DateTime.Now;
                    _context.Settings.Update(setting);
                }

                await _context.SaveChangesAsync();
            }
            //Quét lại bảng Queue để xác định những khách hàng đặt lịch ở tương lai
            var remainningEntries = _context.Queues.ToList();
            if (remainningEntries.Any())
            {
                var tomorrowStart = DateTime.Today.AddDays(1); // 00:00:00 của ngày mai
                                                               //var tomorrowEnd = tomorrowStart.AddDays(1).AddTicks(-1); // 23:59:59 của ngày mai
                var tomorrowEntries = _context.Queues
                .Where(q => q.AppointmentDate.Date == tomorrowStart.Date)
                .ToList();
                if (tomorrowEntries.Any())
                {
                    foreach (var entry in tomorrowEntries)
                    {
                        // Lấy Counter tương ứng
                        var counter = _context.Counters.FirstOrDefault(c => c.Id == entry.CounterId && c.IsActive);
                        if (counter != null)
                        {
                            // Cập nhật thông tin Counter
                            counter.CurrentNumber = 0; // Reset số hiện tại
                            counter.TotalCount += 1; // Tăng tổng số khách hàng
                        }
                    }

                    // Lưu các thay đổi
                    await _context.SaveChangesAsync();
                }
            }
            // Gọi phương thức thống kê hàng ngày ngay lập tức
            await _getDataReportService.GenerateDailyReportAsync(cancellationToken);
        }
    }
}
