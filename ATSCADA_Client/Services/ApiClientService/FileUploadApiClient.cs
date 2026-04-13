using ATSCADA_Client.Interfaces.Client;
using ATSCADA_Client.Response;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Client.Services.ApiClientService
{
    public class FileUploadApiClient : IFileUploadApiClient
    {
        private readonly HttpClient _httpClient;

        public FileUploadApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<string> UploadFile(IBrowserFile file)
        {
            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024)); // 50MB limit
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.Name);

            var response = await _httpClient.PostAsync("api/FileUpload/upload", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<FileUploadResponse>();
            return result?.Path; // Assuming your API returns the file path
        }
    }
    
}
