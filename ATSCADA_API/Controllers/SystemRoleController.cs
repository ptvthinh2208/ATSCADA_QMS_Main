using ATSCADA_Library.Interfaces.Server;
using ATSCADA_Library.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ATSCADA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemRoleController : ControllerBase
    {
        private readonly ISystemRoleRepository? _systemRoleRepository;

        public SystemRoleController(ISystemRoleRepository systemRoleRepository)
        {
            _systemRoleRepository = systemRoleRepository;
        }
        [HttpGet("get-all-systemrole")]
        public async Task<IActionResult> GetAllSystemRole() 
        {
            var listRoles = await _systemRoleRepository!.GetAllRoleAsync();
            return Ok(listRoles);
        }

    }
}
