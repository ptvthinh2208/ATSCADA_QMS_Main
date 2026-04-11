using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Interfaces.Client
{
    public interface IUserApiClient
    {
        Task<PagedList<ApplicationUser>> GetAllUser(PagingParameters paging);
        Task<ApplicationUser> GetUserById(long id);
        Task<ApplicationUser> GetUserByUserName(string userName);
        Task<bool> CreateNewUser(ApplicationUser user);
        Task<bool> UpdateUser(ApplicationUser model);
        Task<bool> UpdatePasswordForChange(ChangePasswordModel model, ApplicationUser currentUser);
        Task<bool> DeleteUser(ApplicationUser model);
    }
}
