using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using ATSCADA_Library.Interfaces.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ATSCADA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportRepository _reportRepository;

        public ReportController(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        [HttpGet("get-all-report-paging")]
        public async Task<IActionResult> GetReports([FromQuery] PagingParameters paging)
        {
            var reports = await _reportRepository.GetAllReportsAsync(paging);
            return Ok(reports);
        }
        [HttpGet("get-all-detailed-report-paging")]
        public async Task<IActionResult> GetDetailedReports([FromQuery] PagingParameters paging)
        {
            var reports = await _reportRepository.GetDetailedReportsAsync(paging);
            return Ok(reports);
        }
        [HttpGet("get-details-report-by-id/{reportId}")]
        public async Task<IActionResult> GetDetailsReportById(int reportId)
        {
            var reports = await _reportRepository.GetDetailsReportByIdAsync(reportId);
            return Ok(reports);
        }
    }
}
