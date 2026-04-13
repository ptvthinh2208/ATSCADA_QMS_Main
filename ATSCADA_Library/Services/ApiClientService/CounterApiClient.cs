using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using ATSCADA_Library.Interfaces.Client;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace ATSCADA_Library.Services.ApiClientService
{
    public class CounterApiClient : ICounterApiClient
    {
        public HttpClient? _httpClient;
        private readonly IConfiguration _configuration;
        public CounterApiClient(HttpClient? httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task<PagedList<Counter>> GetAllCounter(PagingParameters paging)
        {
            var queryStringParam = new Dictionary<string, string>
            {
                ["pageNumber"] = paging.PageNumber.ToString(),
                ["pageSize"] = paging.PageSize.ToString(),
                ["sortBy"] = paging.SortBy,
                ["sortOrder"] = paging.SortOrder,
                ["searchTerm"] = paging.SearchTerm!,
            };
            string url = QueryHelpers.AddQueryString("api/Counter/get-all-counter", queryStringParam!);
            var list = await _httpClient!.GetFromJsonAsync<PagedList<Counter>>(url);
            return list!;
        }
        public async Task<Counter> GetCounterByServiceId(long ServiceId)
        {
            try
            {
                var result = await _httpClient!.GetFromJsonAsync<Counter>($"api/Counter/get-counter-by-ServiceID/{ServiceId}");

                return result!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null!;
            }
        }
        public async Task<List<Counter>> GetListCounterByServiceId(long ServiceId)
        {
            try
            {
                var result = await _httpClient!.GetFromJsonAsync<List<Counter>>($"api/Counter/get-list-counter-by-ServiceID/{ServiceId}");

                return result!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null!;
            }
        }
        public async Task<Counter> GetCounterById(int counterId)
        {
            try
            {
                var result = await _httpClient!.GetFromJsonAsync<Counter>($"api/Counter/get-counter-by-id/{counterId}");
                return result!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null!;
            }
        }
        public async Task<List<Counter>> GetCountersByIds(IEnumerable<int> counterIds)
        {
            var response = await _httpClient!.PostAsJsonAsync("api/Counter/get-counters-by-ids", counterIds);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Counter>>() ?? new List<Counter>();
        }

        public async Task<bool> UpdateCallNext(long ServiceId)
        {
            try
            {
                var result = await _httpClient!.PutAsJsonAsync<Counter>($"api/Counter/update-call-next/{ServiceId}", null!);
                return result.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return false!;
            }
        }
        
        public async Task<bool> UpdateCallNextNumberAsync(int counterId, int currentOrderNumber)
        {
            var response = await _httpClient!.PutAsJsonAsync($"api/Counter/update-call-next/{counterId}", currentOrderNumber);

            if (response.IsSuccessStatusCode)
            {
                // Kiểm tra nếu API trả về thành công
                return true;
            }
            else
            {
                // Xử lý nếu API trả về lỗi
                return false;
            }
        }


        public async Task<bool> CreateTextCallCurrentNumber(string counterName, string code ,int orderNumber, string nameClient)
        {
            if (orderNumber != 0)
            {
                var stringInput = $"Mời quý khách {nameClient} với mã khách hàng {code}-{orderNumber} tới {counterName}";
                var response = await _httpClient!.PostAsJsonAsync("api/queuespeech/create", stringInput);
                return response.IsSuccessStatusCode;
            }
            else return false;


        }
        public async Task<Counter> CreateNewCounter(CounterDto dto)
        {
            //Send a POST request => status code 200
            var response = await _httpClient!.PostAsJsonAsync("api/Counter", dto);
            // Kiểm tra nếu mã trạng thái là thành công
            if (response.IsSuccessStatusCode)
            {
                // Đọc và trả về dữ liệu nếu thành công
                var result = await response.Content.ReadFromJsonAsync<Counter>();
                return result!;
            }
            else return null!;
        }
        public async Task<Counter> UpdateCounter(CounterDto dto)
        {
            try
            {
                var response = await _httpClient!.PutAsJsonAsync($"api/Counter/update-counter-by-id/{dto.Id}", dto);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<Counter>();
                return result!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null!;
            }
        }
        public async Task<bool> UpdateCounterAvgTimeAsync(int counterId, string averageServiceTime)
        {
            // Tạo payload từ chuỗi averageServiceTime
            var payload = JsonContent.Create(averageServiceTime);

            // Gửi yêu cầu PATCH
            var response = await _httpClient!.PatchAsync($"api/Counter/update-counter-avgtime-by-id/{counterId}", payload);

            return response.IsSuccessStatusCode;
        }


    }
}
