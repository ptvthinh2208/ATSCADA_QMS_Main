using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using ATSCADA_Library.Interfaces.Client;
using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Services.ApiClientService
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
