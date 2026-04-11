using ATSCADA_Client.Interfaces.Client;
using ATSCADA_Library.DTOs;
using System.Net.Http.Json;

namespace ATSCADA_Client.Services.ApiClientService
{
    public class ZnsConfigApiClient : IZnsConfigApiClient
    {
        public HttpClient? _httpClient;
        public ZnsConfigApiClient(HttpClient? httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ZnsInfoDto>> GetZnsInfoAsync()
        {
            var list = await _httpClient!.GetFromJsonAsync<List<ZnsInfoDto>>("api/znsconfig/get-all-znsinfo");
            return list!;
        }
        public async Task UpdateZnsInfoAsync(ZnsInfoDto znsInfoDto)
        {
            var response = await _httpClient!.PatchAsJsonAsync($"api/znsconfig", znsInfoDto);
            response.EnsureSuccessStatusCode();
        }

    }
}
