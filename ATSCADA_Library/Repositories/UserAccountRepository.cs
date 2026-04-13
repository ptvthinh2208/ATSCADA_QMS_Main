using ATSCADA_Library.Data;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.DTOs.Response;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers;
using ATSCADA_Library.Interfaces.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Repositories
{
    public class UserAccountRepository (IOptions<JWTSection> config, ATSCADADbContext aTSCADADbContext) : IUserAccount
    {
        public async Task<LoginResponse> SignInAsync(LoginRequest user)
        {
            
            // When verifying the password, use BCrypt.Verify
            
            if (user is null) return new LoginResponse { Successful = false, Message = "Vui lòng nhập user name" };

            var applicationUser = await FindUserByEmail(user.Email!);
            if (applicationUser is null) 
                return new LoginResponse { Successful = false, Message = "Không tìm thấy User" };
            
            if (!BCrypt.Net.BCrypt.Verify(user.Password, applicationUser.Password))
                return new LoginResponse { Successful = false, Message = "Email/password không đúng" };
            //if (applicationUser.Status)
            //    return new LoginResponse { Successful = false, Message = "Tài khoản đang được đăng nhập ở nơi khác" };

            var getRoleName = await aTSCADADbContext.SystemRoles.FirstOrDefaultAsync(x => x.Id == applicationUser.SystemRoleId);
            if(getRoleName is null)
                return new LoginResponse { Successful = false, Message = "Không tìm thấy quyền truy cập" };
            // Tạo token JWT cho người dùng
            string jwtToken = GenerateToken(applicationUser, getRoleName!.Name!);
            // Cập nhật trạng thái người dùng thành 1 (đang đăng nhập)
            //applicationUser.Status = true;
            aTSCADADbContext.ApplicationUsers.Update(applicationUser);
            await aTSCADADbContext.SaveChangesAsync();

            return new LoginResponse { Successful = true, Message = "Login successfully", Token = jwtToken, RoleUser = applicationUser.SystemRoleId, CounterId = applicationUser.CounterId };
        }
        public async Task<LoginResponse> SignOutAsync(string email)
        {
            var applicationUser = await FindUserByEmail(email);
            if (applicationUser is null)
                return new LoginResponse { Successful = false, Message = "Không tìm thấy User" };

            // Đặt lại Status về false khi đăng xuất
            applicationUser.Status = false;
            aTSCADADbContext.ApplicationUsers.Update(applicationUser);
            await aTSCADADbContext.SaveChangesAsync();

            return new LoginResponse { Successful = true, Message = "Logged out successfully", Token = "" };
        }
        private string GenerateToken(ApplicationUser user, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Value.Key!));
            var credentials = new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256);
            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.CounterId.ToString()),
                new Claim(ClaimTypes.Name,user.FullName!),
                new Claim(ClaimTypes.Email,user.UserName!),
                new Claim(ClaimTypes.Role,role!),
            };
            var token = new JwtSecurityToken(
                issuer: config.Value.Issuer,
                audience: config.Value.Audience,
                claims: userClaims,
                //expires: DateTime.Now.AddMinutes(8),
                expires: DateTime.Now.AddHours(8),//Thời gian hết hạn của Authen Token
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<ApplicationUser> FindUserByEmail(string email)
        {
            var result = await aTSCADADbContext.ApplicationUsers.FirstOrDefaultAsync(x => x.UserName!.ToLower()!.Equals(email!.ToLower()));
            return result!;
        }

    }
}
