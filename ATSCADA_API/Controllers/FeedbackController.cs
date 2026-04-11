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
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackRepository? _feedbackRepository;

        public FeedbackController(IFeedbackRepository? feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }
        [HttpGet("get-all-feedback")]
        public async Task<IActionResult> GetAllFeedback([FromQuery] PagingParameters paging)
        {
            var result = await _feedbackRepository!.GetAllFeedbacksAsync(paging);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> CreateFeedback([FromBody] FeedbackDto dto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var res = await _feedbackRepository!.CreateFeedbackAsync(dto);
                    return Ok(res);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
    }
}
