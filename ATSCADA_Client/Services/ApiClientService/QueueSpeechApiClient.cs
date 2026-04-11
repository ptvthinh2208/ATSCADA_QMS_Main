using ATSCADA_Client.Interfaces.Client;
using ATSCADA_Library.Entities;
using System.Net.Http.Json;

namespace ATSCADA_Client.Services.ApiClientService
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
            string protocol = _configuration.GetValue<string>("ApiTTS:Protocol")!;
            string ipAddress = _configuration.GetValue<string>("ApiTTS:IpAddress")!;
            string port = _configuration.GetValue<string>("ApiTTS:Port")!;

            string apiUrl = $"{protocol}://{ipAddress}:{port}/api/v1/Tts/tts";
            //string url = "http://113.161.76.105:5000/api/v1/Tts/tts";
            var bodyContent = new { text = stringInput };
            var response = await _httpClient!.PostAsJsonAsync(apiUrl, bodyContent);
            string audioUrl = "";
            if (response.IsSuccessStatusCode)
            {
                var base64String = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (base64String != null && base64String.TryGetValue("base64_Audio", out var base64Audio))
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
