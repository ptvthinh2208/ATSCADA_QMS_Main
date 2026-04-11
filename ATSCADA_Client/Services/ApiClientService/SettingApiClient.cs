using ATSCADA_Client.Interfaces.Client;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using System.Net.Http.Json;

namespace ATSCADA_Client.Services
{
    public class SettingApiClient : ISettingApiClient
    {
        public HttpClient? _httpClient { get; set; }
        public SettingApiClient(HttpClient? httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<Setting> GetAllSettings()
        {
            var result = await _httpClient!.GetFromJsonAsync<Setting>("api/setting");
            return result!;
        }

        public async Task UpdateSettingAsync(long id, SettingUpdateRequest updateRequest)
        {
            var response = await _httpClient!.PatchAsJsonAsync($"api/setting/1", updateRequest);
            response.EnsureSuccessStatusCode();
        }
       
        public async Task<bool> UpdateZNSTimeNotification(int znsTimeNotification)
        {
            var updateData = new Setting
            {
                //ZNSTimeNotification = znsTimeNotification
            };
            var result = await _httpClient!.PatchAsJsonAsync("api/setting/ZNS-Time-Notification", updateData);
            return result.IsSuccessStatusCode;
        }

        public async Task<bool> RunScheduledTaskNow()
        {
            var response = await _httpClient!.PostAsync("api/Setting/run-now", null);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                Console.WriteLine("Thực thi thất bại.");
                return false;
            }
        }
    }
}
