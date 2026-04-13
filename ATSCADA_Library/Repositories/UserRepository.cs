using ATSCADA_Library.Data;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using ATSCADA_Library.Helpers.Sorting;
using ATSCADA_Library.Interfaces.Server;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ATSCADADbContext _context;
        public UserRepository(ATSCADADbContext context)
        {
            _context = context;
        }

        public async Task<PagedList<ApplicationUser>> GetUserList(PagingParameters paging)
        {
            // Start building the query by searching and optionally applying a date range filter
            var query = _context.ApplicationUsers
                                .Search(paging.SearchTerm!, "FullName")
                                .AsQueryable();
            // Apply sorting (descending or ascending)
            if (paging.SortOrder == "desc")
            {
                var orderByDesc = string.Concat(paging.SortBy, " desc");
                query = query.Sort(orderByDesc);  // Assuming Sort extension method supports sorting by dynamic fields
            }
            else
            {
                query = query.Sort(paging.SortBy);  // Default sort order is ascending
            }

            // If PageSize is 0 or not provided, return all items without paging
            if (paging.PageSize == 0)
            {
                // Return all records without paging
                var fullList = await query.ToListAsync();
                return new PagedList<ApplicationUser>(fullList, fullList.Count, 1, fullList.Count);
            }

            // Execute the query and paginate the results
            var list = await query.ToListAsync();
            return PagedList<ApplicationUser>.ToPagedList(list, paging.PageNumber, paging.PageSize);
        }
        public async Task<ApplicationUser> GetUserById(long id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid ID provided", nameof(id));
            }

            var user = await _context.ApplicationUsers.FindAsync(id);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }

            return user;
        }
        public async Task<ApplicationUser> GetUserByUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException("Invalid username provided", nameof(userName));
            }

            var user = await _context.ApplicationUsers
                                     .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with username '{userName}' not found.");
            }

            return user;
        }
        public async Task<ApplicationUser> UpdateUser(ApplicationUser model)
        {

            _context.ApplicationUsers.Update(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<ApplicationUser> CreateNewUser(ApplicationUser user)
        {
            var existingUser = await _context.ApplicationUsers
                                             .FirstOrDefaultAsync(u => u.UserName == user.UserName);
            if (existingUser != null)
            {
                throw new Exception("A user with this user name already exists.");
            }

            // 2. Validate required fields (e.g., FullName, Email, Password)
            if (string.IsNullOrEmpty(user.FullName) || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Password))
            {
                throw new ArgumentException("FullName, Email, and Password are required.");
            }
            //user.CreatedBy = user.CreatedBy;
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.CreatedDate = DateTime.Now;
            await _context.ApplicationUsers.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }
        public async Task<bool> VerifyPasswordForChange(ChangePasswordModel model, ApplicationUser currentUser)
        {
            // Kiểm tra mật khẩu hiện tại
            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, currentUser.Password))
            {
                return false; // Mật khẩu hiện tại không khớp
            }

            // Kiểm tra mật khẩu mới có khớp với ConfirmPassword không
            if (model.NewPassword != model.ConfirmPassword)
            {
                return false; // Mật khẩu mới và mật khẩu xác nhận không khớp
            }

            // Thay đổi mật khẩu sau khi xác minh các điều kiện
            currentUser.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            // Cập nhật mật khẩu vào cơ sở dữ liệu
            _context.ApplicationUsers.Update(currentUser);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task DeleteUser(ApplicationUser user)
        { 
            _context.ApplicationUsers.Remove(user);
            await _context.SaveChangesAsync();

        }
    }
}
