using ATSCADA_Library.Entities;
using ATSCADA_Library.Interfaces.Client;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.Extensions.Configuration;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Services.ApiClientService
{
    public class QueueSpeechApiClient : IQueueSpeechApiClient
    {
        public HttpClient? _httpClient;
        private readonly IConfiguration _configuration;
        public QueueSpeechApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task<bool> CreateQueueSpeech(QueueSpeech model)
        {
            var result = await _httpClient!.PostAsJsonAsync("api/QueueSpeech/create", model);
            return result.IsSuccessStatusCode;
        }

        public async Task<List<QueueSpeech>> GetQueueSpeechList()
        {
            try
            {
                var list = await _httpClient!.GetFromJsonAsync<List<QueueSpeech>>("api/QueueSpeech/get-all");
                return list ?? new List<QueueSpeech>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return new List<QueueSpeech>();
            }
        }
        public async Task<string> ConvertStringToBase64(string stringInput)
        {
            // Retrieve IP and Port from appsettings.json
            string protocol = _configuration.GetValue<string>("ApiServer:Protocol")!;
            string ipAddress = _configuration.GetValue<string>("ApiServer:IpAddress")!;
            string port = _configuration.GetValue<string>("ApiServer:Port")!;

            string apiUrl = $"{protocol}://{ipAddress}:{port}/tts";
            var bodyContent = new { text = stringInput };
            var response = await _httpClient!.PostAsJsonAsync(apiUrl, bodyContent);
            string audioUrl = "";
            if (response.IsSuccessStatusCode)
            {
                var base64String = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (base64String != null && base64String.TryGetValue("base64_audio", out var base64Audio))
                {
                    audioUrl = $"data:audio/wav;base64,{base64Audio}";
                }
                return audioUrl;
            }
            return string.Empty; ;
        }

        public async Task<bool> UpdateQueueSpeechById(QueueSpeech request)
        {
            var result = await _httpClient!.PutAsJsonAsync($"api/Queuespeech/update-queuespeech/{request.Id}", request);
            return result.IsSuccessStatusCode;
        }
    }
}
