using ATSCADA_Client.Response;
using ATSCADA_Library.DTOs;

namespace ATSCADA_Client.Services.Authenticate
{
    public interface IAuthService
    {
        Task<LoginResponse> Login(LoginRequest loginRequest);
        Task Logout(string email);
    }
}
