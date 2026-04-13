
using System;
using System.Text.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ATSCADA_Library.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ATSCADA_Library.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ATSCADA_Library.Interfaces.Server;


namespace ATSCADA_Library.Services.BackgroundServices
{
    public class ZNSZaloBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ZNSZaloBackgroundService(IServiceScopeFactory scopeFactory
            )
        {
            _scopeFactory = scopeFactory;
            //_zaloApiService = zaloApiService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {

                // Tạo phạm vi mới cho mỗi lần thực hiện công việc
                using (var scope = _scopeFactory.CreateScope())
                {
                    DateTime today = DateTime.Now.Date;
                    DateTime currentTime = DateTime.Now;
                    var dbContext = scope.ServiceProvider.GetRequiredService<ATSCADADbContext>();
                    //var setting = await dbContext.Settings.ToListAsync();

                    double znsTimeNoti = 15; //thông báo trước 15phut
                    //Điều kiện để gửi thông báo tới khách hàng trong bảng Queue
                    var notifications = await dbContext.Queues
                                            .Where(n => n.Status == "Waiting" && n.isNotified == false
                                            && (n.AppointmentDate.Date == today)
                                            && n.AppointmentTime <= currentTime.AddMinutes(znsTimeNoti))
                                            .ToListAsync(stoppingToken);

                    //Gửi ZNS thông báo sắp tới lượt
                    var zaloApiService = scope.ServiceProvider.GetRequiredService<ZaloApiService>();

                    if (notifications.Count > 0)
                    {
                        foreach (var item in notifications)
                        {
                            //var selectedCounter = dbContext.Counters.Find(item.CounterId);
                            //var response = await zaloApiService.SendZNSNotificationAsync(item, selectedCounter!);
                            ////if (!response)
                            ////{
                            ////    response = await zaloApiService.SendZNSNotificationAsync(item, selectedCounter!);
                            ////}
                            ////Cập nhật trạng thái isNotfiied
                            //if (response)
                            //{
                            //    item.isNotified = true;
                            //    dbContext.Queues.Update(item);
                            //    await dbContext.SaveChangesAsync(stoppingToken);
                            //}

                        }
                    }

                }
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); // Kiểm tra mỗi phút
            }
        }

       

    }
}

