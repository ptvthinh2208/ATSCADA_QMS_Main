using ATSCADA_API.Hubs;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using ATSCADA_Library.Interfaces.Server;
using ATSCADA_Library.Repositories;
using ATSCADA_Library.Services.ApiService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI.Common;

namespace ATSCADA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet("get-all-user")]
        public async Task<IActionResult> GetAll([FromQuery] PagingParameters paging)
        {
            var users = await _userRepository.GetUserList(paging);
            return Ok(users);
        }
        [HttpGet("get-user-by-id/{id}")]
        public async Task<IActionResult> GetUserById(long id)
        {
            var users = await _userRepository.GetUserById(id);
            return Ok(users);
        }
        [HttpGet("get-user-by-username/{userName}")]
        public async Task<IActionResult> GetUserByUserName(string userName)
        {
            var users = await _userRepository.GetUserByUserName(userName);
            return Ok(users);
        }
        [HttpPost]
        public async Task<IActionResult> CreateNewUser ([FromBody] ApplicationUser model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var res = await _userRepository.CreateNewUser(model);
                    return Ok(res);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUser([FromBody] ApplicationUser request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userFromDB = await _userRepository.GetUserById(request.Id);

            if (userFromDB == null)
            {
                return NotFound($"{request.Id} is not found");
            }

            userFromDB.FullName = request.FullName;
            userFromDB.UserName = request.UserName;
            userFromDB.SystemRoleId = request.SystemRoleId;
            userFromDB.LastUpdatedBy = request.LastUpdatedBy;
            userFromDB.LastUpdatedDate = DateTime.Now;

            if (request.SystemRoleId == 1)
            {
                userFromDB.CounterId = 0;
            }
            else
            {
                userFromDB.CounterId = request.CounterId;
            }
            // Check if password is provided and update it
            //if (!string.IsNullOrWhiteSpace(request.Password))
            //{
            //    userFromDB.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            //}
            var result = await _userRepository.UpdateUser(userFromDB);
            return Ok(result);
        }
        [HttpPut("update-password-user")]
        public async Task<IActionResult> UpdatePasswordUser([FromBody] UpdatePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var checkPassword = await _userRepository.VerifyPasswordForChange(request.PasswordModel, request.User);

            if (checkPassword)
            {
                return Ok(new { Message = "Password updated successfully." });
            }
            else
            {
                return BadRequest(new { Message = "Password update failed. Please check your current password and confirmation." });
            }
        }
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid user ID.");
                }

                var user = await _userRepository.GetUserById(id);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                await _userRepository.DeleteUser(user);

                return Ok("User deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
