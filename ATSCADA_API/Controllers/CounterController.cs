using ATSCADA_Library.DTOs;
using ATSCADA_Library.Helpers.Pagination;
using ATSCADA_Library.Services.ApiService;
using ATSCADA_Library.Services.ApiService.Interfaces;
using ATSCADA_Library.Services.ApiService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ATSCADA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CounterController : ControllerBase
    {
        private readonly CounterService _counterService;
        private readonly IModbusService _modbusService;

        public CounterController(CounterService counterService, IModbusService modbusService)
        {
            _counterService = counterService;
            _modbusService = modbusService;
        }
        //Lấy danh sách Counter với phân trang
        [HttpGet("get-all-counter")]
        public async Task<IActionResult> GetAllCounter([FromQuery] PagingParameters paging)
        {
            var result = await _counterService.GetAllCounterAsync(paging);
            if (result != null)
            {
                return Ok(result);
            }
            return NotFound();
        }
        //Check lại hàm này!!!
        //Lấy danh sách Counter theo ServiceID
        [HttpGet("get-counter-by-ServiceID/{ServiceId}")]
        public async Task<IActionResult> GetCounterByServiceId(long ServiceId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _counterService!.GetCounterByServiceIdAsync(ServiceId);
            if (result != null)
            {
                if (result.IsActive == true)
                {
                    return Ok(result);
                }
            }
            return Ok();
        }

        //Lấy danh sách Counter theo ServiceID
        [HttpGet("get-list-counter-by-ServiceID/{ServiceId}")]
        public async Task<IActionResult> GetListCounterByServiceId(long ServiceId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            //Chỉ lấy những Counter Active = true
            var result = await _counterService!.GetListCounterByServiceIdAsync(ServiceId);
            return Ok(result);
        }
        //Lấy Counter theo CounterID
        [HttpGet("get-counter-by-id/{id}")]
        public async Task<IActionResult> GetCounterByServiceId(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _counterService!.GetCounterByIdAsync(id);
            if (result != null)
            {
                if (result.IsActive == true)
                {
                    return Ok(result);
                }
            }
            return Ok();
        }
        [HttpPost("get-counters-by-ids")]
        public async Task<IActionResult> GetCountersByIds([FromBody] List<int> ids)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var results = await _counterService!.GetCountersByIdsAsync(ids);
            if (results != null && results.Any())
            {
                var activeCounters = results.Where(c => c.IsActive).ToList();
                return Ok(activeCounters);
            }

            return Ok(new List<CounterDto>()); // Trả về danh sách rỗng nếu không có Counter
        }
        [HttpPost]
        public async Task<IActionResult> CreateNewCounter([FromBody] CounterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _counterService!.CreateNewCounter(dto);
            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest("A counter with this name already exists.");
        }
        [HttpPut("update-counter-by-id/{id}")]
        public async Task<IActionResult> UpdateCounter(long counterId,[FromBody] CounterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (dto.Id == 0)
            {
                return NotFound($"{dto.Id} is not found");
            }
            var result = await _counterService.UpdateCounterByIdAsync(dto);

            return Ok(result);
        }
        [HttpPatch("update-counter-avgtime-by-id/{counterId}")]
        public async Task<IActionResult> UpdateAvgTimeCounter(int counterId, [FromBody] string averageServiceTimeString)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (counterId == 0)
            {
                return NotFound($"{counterId} is not found");
            }

            // Parse chuỗi thành TimeSpan
            if (!TimeSpan.TryParse(averageServiceTimeString, out var averageServiceTime))
            {
                return BadRequest("Invalid time format. Expected format: hh:mm:ss");
            }

            var result = await _counterService.UpdateAvgCounterByIdAsync(counterId, averageServiceTime);

            return Ok(result);
        }



        [HttpPut("update-call-next/{counterId}")]
        public async Task<IActionResult> UpdateCallNextNumber(int counterId, [FromBody] int currentOrderNumber)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _counterService!.GetCounterByIdAsync(counterId);
            if (result != null)
            {
                if (result.IsActive == true && result.ModbusId != 0)
                {
                    var counter = await _counterService.UpdateCounterNumberByIdAsync(counterId, currentOrderNumber);
                    //if (counter != null)
                    //{
                    //    await _modbusService.TransmitOrderNumberAsync(counter.Id, counter.ModbusId);
                    //}
                    return Ok(counter);
                }
            }

            return BadRequest();
        }

        //[HttpPut("update-call-next/{counterId}")]
        //public async Task<IActionResult> UpdateCallNextNumber(int counterId, [FromBody] int currentOrderNumber)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);
        //    var result = await _counterService!.GetCounterByIdAsync(counterId);

        //    if (result != null)
        //    {
        //        if (result.IsActive == true)
        //        {
        //            var counter = await _counterService.UpdateCounterNumberByIdAsync(counterId, currentOrderNumber);
        //            return Ok(counter);
        //        }
        //    }
        //    return BadRequest();
        //}
    }
}
