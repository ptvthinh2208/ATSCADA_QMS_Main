using ATSCADA_Client.Pages;
using ATSCADA_Library.Data;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Interfaces.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;

namespace ATSCADA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(IUserAccount accountInterface) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> SignInAsync(LoginRequest user)
        {
            if (user == null) return BadRequest("User không được để trống");
            var result = await accountInterface.SignInAsync(user);
            if (result.Successful)
            {
                return Ok(result);
            }
            else return BadRequest(result);
        }
        [HttpPost("logout")]
        public async Task<IActionResult> SignOutAsync(string email)
        {
            var applicationUser = await accountInterface.SignOutAsync(email);
            if (applicationUser is null)
                return BadRequest("Không tìm thấy User");
            if (applicationUser.Successful)
            {
                return Ok(applicationUser);
            }
            else return BadRequest(applicationUser);
        }
    }
}
