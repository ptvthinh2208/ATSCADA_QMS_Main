using ATSCADA_API.Hubs;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Interfaces.Server;
using ATSCADA_Library.Repositories;
using ATSCADA_Library.Services.ApiService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ATSCADA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueueSpeechController : ControllerBase
    {
        private readonly IQueueSpeech _queueSpeechRepository;
        private readonly IHubContext<QueueHub> _hubContext; // Inject SignalR Hub Context
        public QueueSpeechController(IQueueSpeech queueSpeechRepository, IHubContext<QueueHub> hubContext)
        {
            _queueSpeechRepository = queueSpeechRepository;
            _hubContext = hubContext;

        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllQueueSpeeches()
        {
            var queueSpeeches = await _queueSpeechRepository.GetAllQueueSpeechAsync();
            return Ok(queueSpeeches);
        }
        //[HttpGet]
        //[Route("api/queuespeech/{id}")]
        //public async Task<IActionResult> GetQueueSpeechById(Guid id)
        //{
        //    var queueSpeech = await _context.QueueSpeeches.FindAsync(id);

        //    if (queueSpeech == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(queueSpeech);
        //}
        [HttpPost("create")]
        public async Task<IActionResult> CreateQueueSpeech([FromBody] string textToSpeech)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var model = new QueueSpeech()
            {
                CreatedDate = DateTime.Now,
                Id = Guid.NewGuid(),
                IsCompleted = false,
                TextToSpeech = textToSpeech
            };
            var createdQueueSpeech = await _queueSpeechRepository.CreateQueueAsync(model);
            // Notify all clients about the queue creation
            await _hubContext.Clients.All.SendAsync("ReceiveMessage");
            return Ok(createdQueueSpeech);
        }
        //Update 
        [HttpPut("update-queuespeech/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] QueueSpeech request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _queueSpeechRepository.UpdateQueueSpeechAsync(request);
            await _hubContext.Clients.All.SendAsync("ReceiveMessage");
            return Ok(result);
        }
    }
}
