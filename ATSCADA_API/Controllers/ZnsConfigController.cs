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
    public class ZnsConfigController : ControllerBase
    {
        private readonly IZnsConfigRepository _znsConfigRepository;
        public ZnsConfigController(IZnsConfigRepository znsConfigRepository)
        {
            _znsConfigRepository = znsConfigRepository;
        }
        [HttpGet("get-all-znsinfo")]
        public async Task<IActionResult> GetAll()
        {
            var znsInfos = await _znsConfigRepository.GetZnsInfoAsync();
            return Ok(znsInfos);
        }
        [HttpPatch]
        public async Task<IActionResult> UpdateZnsInfo([FromBody] ZnsInfoDto znsInfoDto)
        {
            try
            {
                await _znsConfigRepository!.UpdateZnsConfigAsync(znsInfoDto);
                return NoContent(); // 204 No Content on success
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
