using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Client.Interfaces.Client;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Json;

namespace ATSCADA_Client.Services.ApiClientService
{
    public class UserApiClient : IUserApiClient
    {
        public HttpClient? _httpClient;
        public UserApiClient(HttpClient? httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> CreateNewUser(ApplicationUser user)
        {
            var result = await _httpClient!.PostAsJsonAsync($"api/User", user);
            return result.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUser(ApplicationUser model)
        {
            // Thực hiện yêu cầu HTTP DELETE
            var response = await _httpClient!.DeleteAsync($"api/User/delete-user/{model.Id}");

            // Kiểm tra xem yêu cầu có thành công không
            if (response.IsSuccessStatusCode)
            {
                return true;  // Xóa thành công
            }

            return false;  // Xóa thất bại
        }

        public async Task<PagedList<ApplicationUser>> GetAllUser(PagingParameters paging)
        {
            var queryStringParam = new Dictionary<string, string>
            {
                ["pageNumber"] = paging.PageNumber.ToString(),
                ["pageSize"] = paging.PageSize.ToString(),
                ["sortBy"] = paging.SortBy,
                ["sortOrder"] = paging.SortOrder,
                ["searchTerm"] = paging.SearchTerm!,
            };
            string url = QueryHelpers.AddQueryString("api/User/get-all-user", queryStringParam!);
            var list = await _httpClient!.GetFromJsonAsync<PagedList<ApplicationUser>>(url);
            return list!;
        }
        public async Task<ApplicationUser> GetUserById(long id)
        {
            var result = await _httpClient!.GetFromJsonAsync<ApplicationUser>($"api/User/get-user-by-id/{id}");
            return result!;
        }
        public async Task<ApplicationUser> GetUserByUserName(string userName)
        {
            var result = await _httpClient!.GetFromJsonAsync<ApplicationUser>($"api/User/get-user-by-username/{userName}");
            return result!;
        }
        public async Task<bool> UpdateUser(ApplicationUser model)
        {
            var response = await _httpClient!.PutAsJsonAsync("api/User/update-user", model);
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> UpdatePasswordForChange(ChangePasswordModel model, ApplicationUser currentUser)
        {
            var updatePasswordRequest = new UpdatePasswordRequest
            {
                PasswordModel = model,
                User = currentUser
            };

            // Gửi yêu cầu PUT đến API
            var response = await _httpClient!.PutAsJsonAsync("api/User/update-password-user", updatePasswordRequest);

            return response.IsSuccessStatusCode;
        }
    }
}
