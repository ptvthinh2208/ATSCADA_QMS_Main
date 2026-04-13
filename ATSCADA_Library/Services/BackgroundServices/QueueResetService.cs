using ATSCADA_Library.Data;
using ATSCADA_Library.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ATSCADA_Library.Services.BackgroundServices
{
    public class QueueResetService
    {
        private readonly ATSCADADbContext _dbContext;

        public QueueResetService(ATSCADADbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task ResetQueuesAsync(CancellationToken stoppingToken)
        {
            //Clear toàn bộ danh sách hàng chờ từ quá khứ đến trước ngày hiện tại ( 00h00 ngày hiện tại )
            var setting = _dbContext.Settings.FirstOrDefault();
            var startTime = DateTime.Today;
            //var endTime = DateTime.Today.AddDays(1).AddTicks(-1);

            var oldEntries = _dbContext.Queues
                .Where(q => q.AppointmentDate < startTime)
                .ToList();

            if (oldEntries.Any())
            {
                // Map to history
                var queueHistories = oldEntries.Select(q => new QueueHistory
                {
                    NameService = q.NameService,
                    OrderNumber = q.OrderNumber,
                    DescriptionService = q.DescriptionService,
                    FullName = q.FullName,
                    PhoneNumber = q.PhoneNumber,
                    AppointmentDate = q.AppointmentDate,
                    Message = q.Message,
                    // Nếu status hiện tại là "Processing" thì chuyển thành "Completed"
                    Status = q.Status == "Processing" ? "Completed" : q.Status,
                    Verified = q.Verified,
                    CounterId = q.CounterId,
                    OriginalCounterId = q.OriginalCounterId,
                    ServiceId = q.ServiceId,
                    AppointmentId = q.AppointmentId,
                    PrintTime = q.PrintTime,
                    isNotified = q.isNotified,
                    Priority = q.Priority,
                    LastTimeUpdated = q.LastTimeUpdated
                }).ToList();

                // Save to history
                _dbContext.QueueHistories.AddRange(queueHistories);

                // Remove from active queues
                _dbContext.Queues.RemoveRange(oldEntries);


            }
            // Reset counters
            var allCounters = _dbContext.Counters.Where(c => c.IsActive).ToList();
            foreach (var counter in allCounters)
            {
                counter.CurrentNumber = 0;
                counter.TotalCount = 0;
            }
            //Cập nhật tổng số phiếu đã in trong ngày về 0
            var allService = _dbContext.Services.Where(x => x.IsActive).ToList();
            foreach (var Service in allService)
            {
                Service.TotalTicketPrint = 0;
            }
            // Cập nhật thời gian thực thi cuối cùng
            if (setting.LastTaskExecuted == null && setting.ScheduledTaskTime != null)
            {
                setting.LastTaskExecuted = DateTime.Now;
                _dbContext.Settings.Update(setting);
            }
            else if (setting.ScheduledTaskTime != null && setting.LastTaskExecuted != null)
            {
                setting.LastTaskExecuted = DateTime.Now;
                _dbContext.Settings.Update(setting);
            }
            await _dbContext.SaveChangesAsync(stoppingToken);
        }

        public async Task SeedDailyQueuesAsync(CancellationToken ct)
        {
            // Tìm dịch vụ đầu tiên đang hoạt động để gán 2000 số
            var firstService = await _dbContext.Services.FirstOrDefaultAsync(s => s.IsActive, ct);
            if (firstService == null)
            {
                // Nếu không có dịch vụ nào active, không thể rải số
                return;
            }

            long serviceId = firstService.Id;
            var values = new StringBuilder();

            for (int i = 1; i <= 2000; i++)
            {
                values.Append($@"(
                    {i},
                    '0',
                    NOW(),
                    '0001-01-01 00:00:00',
                    'Waiting',
                    1,
                    {serviceId},
                    0,
                    0,
                    0,
                    NOW(),
                    0,
                    0,
                    '0001-01-01 00:00:00'
                )");

                if (i < 2000)
                {
                    values.Append(",");
                }
            }

            var insertSql = $@"
                INSERT INTO queues 
                (OrderNumber, DescriptionService, AppointmentDate, AppointmentTime, Status, Verified, 
                 ServiceId, CounterId, OriginalCounterId, AppointmentId, PrintTime, isNotified, Priority, LastTimeUpdated)
                VALUES {values};";

            try
            {
                await _dbContext.Database.ExecuteSqlRawAsync(insertSql, ct);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần, ở đây em giữ nguyên để không làm phức tạp hóa
                Console.WriteLine($"Error seeding data: {ex.Message}");
            }
        }
    }

}
