using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using Newtonsoft.Json;

namespace ATSCADA_WinForms.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        public string? Token { get; private set; }
        public int CounterId { get; private set; }

        public ApiService(string baseUrl)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var loginRequest = new LoginRequest { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/Authentication/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = Newtonsoft.Json.Linq.JObject.Parse(content);
                bool success = (bool?)json["successful"] ?? (bool?)json["Successful"] ?? false;
                
                if (success)
                {
                    Token = (json["token"] ?? json["Token"])?.ToString();
                    CounterId = (int?) (json["counterId"] ?? json["CounterId"]) ?? 0;
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                    return true;
                }
            }
            return false;
        }

        public async Task<Counter?> GetCounterAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Counter>($"api/Counter/get-counter-by-id/{id}");
        }

        public async Task<Modbus?> GetModbusAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Modbus>($"api/Modbus/{id}");
        }

        public async Task<Queue?> CallNextAsync(int counterId, long serviceId)
        {
            var request = new { ServiceId = serviceId };
            var response = await _httpClient.PostAsJsonAsync($"api/queue/counter/{counterId}/call-next", request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = Newtonsoft.Json.Linq.JObject.Parse(content);
                bool success = (bool?)json["success"] ?? (bool?)json["Success"] ?? false;

                if (success)
                {
                    var queueToken = json["queue"] ?? json["Queue"];
                    if (queueToken != null)
                    {
                        return queueToken.ToObject<Queue>();
                    }
                }
            }
            return null;
        }

        public async Task<Queue?> CallPreviousAsync(int counterId, long serviceId)
        {
            var request = new { ServiceId = serviceId };
            var response = await _httpClient.PostAsJsonAsync($"api/queue/counter/{counterId}/call-previous", request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = Newtonsoft.Json.Linq.JObject.Parse(content);
                bool success = (bool?)json["success"] ?? (bool?)json["Success"] ?? false;

                if (success)
                {
                    var queueToken = json["queue"] ?? json["Queue"];
                    if (queueToken != null)
                    {
                        return queueToken.ToObject<Queue>();
                    }
                }
            }
            return null;
        }

        public async Task<bool> RecallAsync(int counterId, int orderNumber, string fullName, string idNum, string counterName, string code)
        {
            // Replicates CreateTextCallCurrentNumber logic
            if (orderNumber == 0) return false;
            
            var mergeText = code + orderNumber.ToString("D3");
            // Simple space insertion for voice
            var cleanText = string.Join(" ", mergeText.ToCharArray());
            var stringInput = $"{cleanText} , vào, {counterName}.";
            
            var response = await _httpClient.PostAsJsonAsync("api/queuespeech/create", stringInput);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateQueueStatusAsync(int id, string status)
        {
            // Fetch first to get existing data (standard pattern in the Blazor app)
            var queue = await _httpClient.GetFromJsonAsync<Queue>($"api/Queue/get-queue-by-id/{id}");
            if (queue != null)
            {
                queue.Status = status;
                var response = await _httpClient.PutAsJsonAsync($"api/Queue/update-queue/{id}", queue);
                return response.IsSuccessStatusCode;
            }
            return false;
        }

        public async Task<bool> TransmitModbusAsync(int counterId, int orderNumber)
        {
            var response = await _httpClient.PostAsync($"api/modbus/transmit/{counterId}/{orderNumber}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateCounterNumberAsync(int counterId, int number)
        {
            // We need an endpoint to update CurrentNumber. 
            // Looking at CounterApiClient, it uses api/Counter/update-call-next/{counterId} with orderNumber
            var response = await _httpClient.PutAsJsonAsync($"api/Counter/update-call-next/{counterId}", number);
            return response.IsSuccessStatusCode;
        }
    }
}
