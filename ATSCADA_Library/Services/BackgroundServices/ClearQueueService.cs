using ATSCADA_Library.Data;
using ATSCADA_Library.Entities;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ATSCADA_Library.Services.BackgroundServices
{
    public class ClearQueueService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ClearQueueService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var queueResetService = scope.ServiceProvider.GetRequiredService<QueueResetService>();
                    var dailyReportService = scope.ServiceProvider.GetRequiredService<DailyReportService>();
                    var scheduledTaskService = scope.ServiceProvider.GetRequiredService<ScheduledTaskService>();
                    var appointmentService = scope.ServiceProvider.GetRequiredService<AppointmentService>();
                    var counterService = scope.ServiceProvider.GetRequiredService<CounterServiceBackground>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ATSCADADbContext>();

                    var setting = await scheduledTaskService.GetSettingsAsync(stoppingToken);
                    if (setting?.ScheduledTaskTime != null)
                    {
                        var now = DateTime.Now;
                        var scheduledTime = setting.ScheduledTaskTime.Value;
                        var lastExecuted = setting.LastTaskExecuted;

                        // Nếu đến giờ đã setup và chưa chạy hôm nay
                        if (now.TimeOfDay >= scheduledTime &&
                            (lastExecuted == null || lastExecuted.Value.Date < now.Date))
                        {
                            // Reset hàng chờ và counters
                            await queueResetService.ResetQueuesAsync(stoppingToken);

                            //// Cập nhật số lượng từ danh sách đặt lịch hôm nay
                            //var futureAppointments = await appointmentService.GetAppointmentsForTodayAsync(stoppingToken);
                            //await counterService.UpdateTotalCountForDateAsync(futureAppointments, stoppingToken);

                            // Tạo báo cáo
                            await dailyReportService.GenerateDailyReportAsync(stoppingToken);

                            // Ghi lại thời gian đã chạy
                            setting.LastTaskExecuted = DateTime.Now;
                            dbContext.Settings.Update(setting);
                            await dbContext.SaveChangesAsync(stoppingToken);
                        }
                    }
                }

                // Kiểm tra lại sau 5 phút
                await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
            }
        }

    }

}

