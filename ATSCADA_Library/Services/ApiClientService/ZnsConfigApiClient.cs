using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using ATSCADA_Library.Interfaces.Client;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Services.ApiClientService
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
