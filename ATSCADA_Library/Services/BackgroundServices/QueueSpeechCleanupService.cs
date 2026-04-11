using ATSCADA_Library.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Services.BackgroundServices
{
    public class QueueSpeechCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public QueueSpeechCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {


                while (!stoppingToken.IsCancellationRequested)
                {
                    Console.WriteLine($"Clear speech Run");
                    await Task.Delay(TimeSpan.FromSeconds(25), stoppingToken); // Chạy mỗi 25 giây

                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ATSCADADbContext>();

                    var now = DateTime.UtcNow;

                    // Lấy các record quá hạn
                    var expiredRecords = dbContext.QueueSpeeches
                     .AsEnumerable() // Chuyển về xử lý trên bộ nhớ
                     .Where(x => x.IsCompleted || (now - x.CreatedDate).TotalSeconds > 30)
                     .ToList();
                    if (expiredRecords != null)
                    {
                        // Xoá các record quá hạn
                        dbContext.QueueSpeeches.RemoveRange(expiredRecords);
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Speech Error:{ex.Message}");
            }
        }
    }
}
