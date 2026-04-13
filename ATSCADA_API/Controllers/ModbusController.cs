using ATSCADA_API.Interfaces;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Interfaces.Server;
using ATSCADA_Library.Services.ApiService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ATSCADA_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModbusController : ControllerBase
    {
        private readonly IModbusService _service;
        private readonly ICounterRepository _counterRepo;

        public ModbusController(IModbusService service, ICounterRepository counterRepo)
        {
            _service = service;
            _counterRepo = counterRepo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ModbusDto>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ModbusDto>> GetById(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] ModbusDto dto)
        {
            await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] ModbusDto dto)
        {
            try
            {
                await _service.UpdateAsync(id, dto);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("transmit/{counterId}/{value}")]
        public async Task<ActionResult> TransmitOrderNumber(int counterId, int value)
        {
            try
            {
                // 1. Tìm thông tin Quầy để lấy ModbusId được cấu hình cho quầy đó
                var counter = await _counterRepo.GetCounterByIdAsync(counterId);
                if (counter == null) 
                    return NotFound($"Không tìm thấy Quầy có ID {counterId}");

                if (counter.ModbusId <= 0)
                    return BadRequest($"Quầy '{counter.Name}' chưa được cấu hình ID bảng LED (ModbusId)");

                // 2. Truyền giá trị (số thứ tự) lên đúng bảng LED của quầy
                await _service.TransmitOrderNumberAsync(counter.ModbusId, (ushort)value);

                return Ok(new { 
                    Message = "Đã gửi tín hiệu tới bảng LED thành công",
                    Counter = counter.Name,
                    ModbusId = counter.ModbusId,
                    ValueSent = value
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (TimeoutException ex)
            {
                return StatusCode(504, ex.Message); // Gateway Timeout
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truyền Modbus: {ex.Message}");
            }
        }
    }
}
