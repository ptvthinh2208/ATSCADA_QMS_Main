using ATSCADA_Library.DTOs;
using ATSCADA_Library.Helpers.Pagination;
using ATSCADA_Library.Interfaces.Server;
using ATSCADA_Library.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ATSCADA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueueHistoryController : ControllerBase
    {
        private readonly IQueueHistoryRepository _queuehistoryRepository;

        public QueueHistoryController(IQueueHistoryRepository queuehistoryRepository)
        {
            _queuehistoryRepository = queuehistoryRepository;  
        }

        [HttpGet("get-all-queue-history-paging")]
        public async Task<IActionResult> GetQueueHistoryList([FromQuery] PagingParameters paging)
        {
            var reports = await _queuehistoryRepository.GetAllQueueHistoryAsync(paging);
            return Ok(reports);
        }
        // POST api/zns/send-messages
        [HttpPost("send-messages-for-customers")]
        public async Task<IActionResult> SendMessages([FromBody] SendZnsRequest request)
        {
            if (request.ListModel == null || string.IsNullOrEmpty(request.TemplateName))
            {
                return BadRequest("Danh sách khách hàng và tên mẫu không được để trống.");
            }
            try
            {
                await _queuehistoryRepository.SendZnsForAllCustomers(request.ListModel, request.TemplateName);
                return Ok("Tin nhắn đã được gửi thành công.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi gửi tin nhắn: {ex.Message}");
            }
        }
        [HttpGet("get-data-chart")]
        public async Task<IActionResult> GetChartData([FromQuery] PagingParameters paging, [FromQuery] string status, [FromQuery] long ServiceId, [FromQuery] int counterId)
        {
            var reports = await _queuehistoryRepository.GetChartDataAsync(paging, status, ServiceId, counterId);
            return Ok(reports);
        }
    }
}
