using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ATSCADA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ConfigurationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("basepath")]
        public IActionResult GetBasePath()
        {
            var basePath = _configuration.GetValue<string>("AppBasePath");
            return Ok(basePath);
        }
    }
}
