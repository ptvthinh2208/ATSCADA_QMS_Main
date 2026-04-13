using ATSCADA_Library.DTOs;
using ATSCADA_Library.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Interfaces.Server
{
    public interface IUserAccount
    {
        Task<LoginResponse> SignInAsync(LoginRequest user);
        Task<LoginResponse> SignOutAsync(string email);
    }
}
