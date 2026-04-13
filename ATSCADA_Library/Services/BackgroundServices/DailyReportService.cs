using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Services.BackgroundServices
{
    public class DailyReportService
    {
        private readonly GetDataReportService _dataReportService;

        public DailyReportService(GetDataReportService dataReportService)
        {
            _dataReportService = dataReportService;
        }

        public async Task GenerateDailyReportAsync(CancellationToken stoppingToken)
        {
            await _dataReportService.GenerateDailyReportAsync(stoppingToken);
        }
    }

}
