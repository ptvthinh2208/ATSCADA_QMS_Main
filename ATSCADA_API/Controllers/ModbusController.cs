using ATSCADA_API.Interfaces;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Services.ApiService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ATSCADA_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModbusController : ControllerBase
    {
        private readonly IModbusService _service;

        public ModbusController(IModbusService service)
        {
            _service = service;
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

        // Custom endpoint to transmit OrderNumber from a Counter to the LED via Modbus
        [HttpPost("transmit/{counterId}/{modbusId}")]
        public async Task<ActionResult> TransmitOrderNumber(int counterId, int modbusId)
        {
            //try
            //{
            //    await _service.TransmitOrderNumberAsync(counterId, modbusId);
            //    return Ok("OrderNumber transmitted successfully");
            //}
            //catch (KeyNotFoundException ex)
            //{
            //    return NotFound(ex.Message);
            //}
            //catch (ArgumentException ex)
            //{
            //    return BadRequest(ex.Message);
            //}
            //catch (Exception ex)
            //{
            //    return StatusCode(500, $"Transmission failed: {ex.Message}");
            //}
            return BadRequest("Direct Modbus transmit is disabled. LED is controlled by LedSyncService.");
        }
    }
}
