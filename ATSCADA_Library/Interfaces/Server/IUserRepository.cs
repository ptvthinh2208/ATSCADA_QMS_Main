using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Interfaces.Server
{
    public interface IUserRepository
    {
        Task<PagedList<ApplicationUser>> GetUserList(PagingParameters paging);
        Task<ApplicationUser> GetUserById(long id);
        Task<ApplicationUser> UpdateUser(ApplicationUser model);
        Task<ApplicationUser> CreateNewUser(ApplicationUser user);
        Task DeleteUser(ApplicationUser user);
        Task<ApplicationUser> GetUserByUserName(string userName);
        Task<bool> VerifyPasswordForChange(ChangePasswordModel model, ApplicationUser currentUser);
    }
}
