using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using ATSCADA_Library.Interfaces.Server;
using Microsoft.AspNetCore.Mvc;

namespace ATSCADA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IQueueRepository _queueRepository;

        public ServiceController(IQueueRepository queueRepository)
        {
            _queueRepository = queueRepository;
        }
        //Get all Service
        [HttpGet("get-all-Service")]
        public async Task<IActionResult> GetAllService()
        {
            var list = await _queueRepository!.GetServiceList();
            return Ok(list);
        }
        //Get all Service
        [HttpGet("get-all-Service-paging")]
        public async Task<IActionResult> GetAllService([FromQuery] PagingParameters paging)
        {

            var paginatedList = await _queueRepository!.GetServiceList(paging);
            return Ok(paginatedList);
        }
        //Get Service by id
        [HttpGet("get-Service-by-id/{id}")]
        public async Task<IActionResult> GetServiceById(long id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _queueRepository!.GetServiceById(id);
            if (result != null)
            {
                return Ok(result);
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> CreateNewService([FromBody] Service model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _queueRepository!.CreateServiceAsync(model);
            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest();
        }
        //Update 
        [HttpPut("update-Service/{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] ServiceDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultFromDb = await _queueRepository.GetServiceById(id);

            if (resultFromDb == null)
            {
                return NotFound($"{id} is not found");
            }
            if (resultFromDb.Name != request.Name)
            {
                var existingService = await _queueRepository.CheckNameService(request);
                if (existingService != null)
                {
                    throw new Exception("A Service with this name already exists.");
                }
            }
            resultFromDb.Name = request.Name;
            resultFromDb.Description = request.Description;
            resultFromDb.IsActive = request.IsActive;
            resultFromDb.LastUpdatedBy = request.LastUpdatedBy;
            resultFromDb.LastUpdatedDate = DateTime.Now;
            var Result = await _queueRepository.UpdateService(resultFromDb);

            return Ok(new Service()
            {
                Id = Result.Id,
                Name = Result.Name,
                Description = Result.Description,
                IsActive = Result.IsActive,
                CreatedDate = Result.CreatedDate
            });
        }
        [HttpPut("update-totalprint-Service/{id}")]
        public async Task<IActionResult> UpdateTotalPrint(long id, [FromBody] ServiceDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultFromDb = await _queueRepository.GetServiceById(id);

            if (resultFromDb == null)
            {
                return NotFound($"{id} is not found");
            }
            if (resultFromDb.TotalTicketPrint != request.TotalPrint)
            {

            }

            var Result = await _queueRepository.UpdateService(resultFromDb);

            return Ok(new Service()
            {
                Id = Result.Id,
                Name = Result.Name,
                Description = Result.Description,
                IsActive = Result.IsActive,
                CreatedDate = Result.CreatedDate
            });
        }
    }
}
