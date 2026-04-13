using ATSCADA_Library.DTOs;
using ATSCADA_Library.DTOs.Response;

namespace ATSCADA_Library.Services.Authenticate
{
    public interface IAuthService
    {
        Task<LoginResponse> Login(LoginRequest loginRequest);
        Task Logout(string email);
    }
}
