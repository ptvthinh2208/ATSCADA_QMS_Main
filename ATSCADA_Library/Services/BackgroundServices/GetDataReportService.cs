using ATSCADA_Library.Data;
using ATSCADA_Library.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATSCADA_Library.Services.BackgroundServices
{
    public class GetDataReportService
    {
        private readonly ATSCADADbContext _dbContext;

        public GetDataReportService(ATSCADADbContext dbContext)
        {
            _dbContext = dbContext;
        }

        //Tạo báo cáo lấy kết quả ngày hôm trước
        public async Task GenerateDailyReportAsync(CancellationToken stoppingToken)
        {
            var today = DateTime.Today.AddDays(-1);
            var tomorrow = today.AddDays(1);
            //Lấy tất cả dữ liệu trong bảng Queue nhưng không lấy Transferred
            var totalPrint = await _dbContext.QueueHistories
                .CountAsync(q => q.PrintTime >= today && q.PrintTime < tomorrow && q.Status != "Transferred", stoppingToken);

            var totalCompleted = await _dbContext.QueueHistories
                .CountAsync(q => q.Status == "Completed" && q.PrintTime >= today && q.PrintTime < tomorrow, stoppingToken);

            var totalMissed = await _dbContext.QueueHistories
                .CountAsync(q => q.Status == "Missed" && q.PrintTime >= today && q.PrintTime < tomorrow, stoppingToken);

            //Nếu có khách hàng vẫn ở trạng thái Waiting cũng sẽ là Cancel
            var totalCancel = await _dbContext.QueueHistories
                .CountAsync(q => q.Status != "Completed" && q.PrintTime >= today && q.PrintTime < tomorrow, stoppingToken);

            var existingReport = await _dbContext.Reports
                .FirstOrDefaultAsync(r => r.CreatedDate == today, stoppingToken);

            if (existingReport == null)
            {
                var report = new Report
                {
                    CreatedDate = today,
                    TotalCompleted = totalCompleted,
                    TotalMissed = 0,
                    TotalCancel = totalCancel,
                    TotalPrint = totalPrint
                };

                _dbContext.Reports.Add(report);
            }
            else
            {
                existingReport.TotalCompleted = totalCompleted;
                existingReport.TotalMissed = 0;
                existingReport.TotalCancel = totalCancel;
                existingReport.TotalPrint = totalPrint;
            }


            await _dbContext.SaveChangesAsync(stoppingToken);
        }
    }

}
