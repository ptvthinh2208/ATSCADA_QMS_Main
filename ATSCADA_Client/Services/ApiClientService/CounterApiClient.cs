using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Client.Interfaces.Client;
using ATSCADA_Client.Services.TTS;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Json;

namespace ATSCADA_Client.Services.ApiClientService
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
        //public async Task<bool> UpdateCallNextNumber(long ServiceId, int orderNumber)
        //{
        //    try
        //    {
        //        //var priorityData = new { orderNumberPriority };  // Wrap in an object
        //        var result = await _httpClient!.PutAsJsonAsync($"api/Counter/update-call-next/{ServiceId}", orderNumber);
        //        return result.IsSuccessStatusCode;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Exception: {ex.Message}");
        //        return false!;
        //    }
        //}
        //public async Task<bool> UpdateCallNextNumberAsync(int counterId, int currentOrderNumber)
        //{
        //    var response = await _httpClient!.PutAsJsonAsync($"api/Counter/update-call-next/{counterId}", currentOrderNumber);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        // Kiểm tra nếu API trả về thành công
        //        return true;
        //    }
        //    else
        //    {
        //        // Xử lý nếu API trả về lỗi
        //        return false;
        //    }
        //}
        public async Task<Counter?> UpdateCallNextNumberAsync(int counterId, int currentOrderNumber)
        {
            var response = await _httpClient!.PutAsJsonAsync($"api/Counter/update-call-next/{counterId}", currentOrderNumber);

            if (response.IsSuccessStatusCode)
            {
                // Deserialize nội dung trả về thành đối tượng Counter
                var counter = await response.Content.ReadFromJsonAsync<Counter>();
                return counter; // Trả về Counter nếu thành công
            }
            else
            {
                // Xử lý lỗi
                Console.WriteLine("Error: Failed to update call next number");
                return null;
            }
        }

        //Hàm tạo chuỗi phát loa
        public async Task<bool> CreateTextCallCurrentNumber(string counterName, string code ,int orderNumber, string nameClient, string vehicleNumber)
        {            if (orderNumber != 0)
            {
                //var cleanVehicleNumber = TextToSpeechService.InsertSpacesBetweenCharacters(vehicleNumber);
                var mergeText = code + orderNumber.ToString("D3");
                var cleanText = TextToSpeechService.InsertSpacesBetweenCharacters(mergeText);
                //var finalVehicleNumber = TextToSpeechService.
                var stringInput = $"{cleanText} , vào, {counterName}.";
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
