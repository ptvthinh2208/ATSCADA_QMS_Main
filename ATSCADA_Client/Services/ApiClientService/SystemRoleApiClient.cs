using ATSCADA_Client.Interfaces.Client;
using ATSCADA_Library.Entities;
using System.Net.Http.Json;

namespace ATSCADA_Client.Services.ApiClientService
{
    public class SystemRoleApiClient : ISystemRoleApiClient
    {
        public HttpClient? _httpClient;
        public SystemRoleApiClient(HttpClient? httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<List<SystemRole>> GetAllSystemRole()
        {
            var list = await _httpClient!.GetFromJsonAsync<List<SystemRole>>("api/SystemRole/get-all-systemrole");
            
            return list!;
        }
    }
}
