using ATSCADA_Client.Helpers;
using ATSCADA_Client.Response;
using ATSCADA_Library.DTOs;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ATSCADA_Client.Services.Authenticate
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public AuthService(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider,
                           ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _localStorage = localStorage;
        }
        public async Task<LoginResponse> Login(LoginRequest loginRequest)
        {

            var result = await _httpClient.PostAsJsonAsync("api/Authentication/login", loginRequest);
            var content = await result.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content,
                new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                });
            if (!result.IsSuccessStatusCode)
            {
                return loginResponse!;
            }
            await _localStorage.SetItemAsync("authToken", loginResponse!.Token);
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(loginRequest.Email!, loginResponse.Token!);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
            return loginResponse;
        }
        public async Task Logout(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                // Gọi API và gửi email của người dùng để xác định tài khoản
                await _httpClient.PostAsync($"api/Authentication/logout?email={email}", null);
            }

            // Xóa token khỏi Local Storage và cập nhật trạng thái xác thực trên client
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("userEmail");
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        //public async Task Logout()
        //{
        //    await _localStorage.RemoveItemAsync("authToken");
        //    ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
        //    _httpClient.DefaultRequestHeaders.Authorization = null;
        //}
    }
}

