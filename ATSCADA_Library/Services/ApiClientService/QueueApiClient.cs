using ATSCADA_Library.DTOs;
using ATSCADA_Library.DTOs.Response;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using ATSCADA_Library.Interfaces.Client;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace ATSCADA_Library.Services.ApiClientService
{
    public class QueueApiClient : IQueueApiClient
    {
        public HttpClient? _httpClient;
        public QueueApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        //MySql hệ cũ
        public async Task<QueueResponse> CallPrintService(long serviceId)
        {
            try
            {
                var result = await _httpClient!.GetFromJsonAsync<QueueResponse>($"api/ServicePrint/get-service-print/{serviceId}");
                return result!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null!;
            }
        }
        #region Queue Table
        //
        public async Task<List<Queue>> GetListAllQueue()
        {
            try
            {
                var list = await _httpClient!.GetFromJsonAsync<List<Queue>>("api/Queue/get-all-queue");
                return list ?? new List<Queue>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return new List<Queue>();
            }
        }
        //
        public async Task<PagedList<QueueHistory>> GetListQueueHistory(PagingParameters paging, DateTime? startDate, DateTime? endDate)
        {
            var queryStringParam = new Dictionary<string, string>
            {
                ["pageNumber"] = paging.PageNumber.ToString(),
                ["pageSize"] = paging.PageSize.ToString(),
                ["sortBy"] = paging.SortBy,
                ["sortOrder"] = paging.SortOrder,
                ["searchTerm"] = paging.SearchTerm!,
            };
            if (startDate.HasValue)
                queryStringParam["startDate"] = startDate.Value.ToString("yyyy-MM-dd");
            if (endDate.HasValue)
                queryStringParam["endDate"] = endDate.Value.ToString("yyyy-MM-dd");
            string url = QueryHelpers.AddQueryString("api/Queue/get-all-report-paging", queryStringParam!);
            var result = await _httpClient!.GetFromJsonAsync<PagedList<QueueHistory>>(url);
            return result!;
        }
        //
        public async Task<bool> CreateQueue(Appointment model)
        {
            var result = await _httpClient!.PostAsJsonAsync("api/Queue", model);
            return result.IsSuccessStatusCode;
        }
        //Gọi api lấy danh sách khách hàng theo Counter Id
        public async Task<List<Queue>> GetQueuesByCounterIdAsync(int counterId)
        {
            var result = await _httpClient!.GetFromJsonAsync<List<Queue>>($"api/Queue/get-queues-by-counter-id?counterId={counterId}");
            return result!;
        }

        public async Task<List<Queue>> GetQueuesByServiceId(long ServiceId)
        {
            var result = await _httpClient!.GetFromJsonAsync<List<Queue>>($"api/Queue/get-all-queue-by-ServiceID/{ServiceId}");
            return result!;
        }
        public async Task<List<Queue>> GetQueuesByStatusesAndServiceIdAsync(List<string> statuses, long ServiceId)
        {
            // Convert statuses to query string format
            string statusQuery = string.Join("&", statuses.Select(status => $"statuses={status}"));

            // Build the full URL
            string url = $"api/Queue/get-queues-by-statuses?{statusQuery}&ServiceId={ServiceId}";

            // Call the API using GetFromJsonAsync
            var result = await _httpClient!.GetFromJsonAsync<List<Queue>>(url);

            return result!;
        }
        public async Task<Queue> GetQueuesByOrderNumberAndCounterIdAsync(int orderNumber, long ServiceId)
        {
            // Build the full URL
            string url = $"api/Queue/get-queue-by-ordernumber-and-ServiceID?orderNumber={orderNumber}&ServiceId={ServiceId}";
            // Call the API using GetFromJsonAsync
            var result = await _httpClient!.GetFromJsonAsync<Queue>(url);
            return result!;
        }
        public async Task<bool> UpdateQueueById(int id, Queue request)
        {
            var result = await _httpClient!.PutAsJsonAsync($"api/Queue/update-queue/{id}", request);
            return result.IsSuccessStatusCode;

        }
        public async Task<Queue?> UpdateTransferQueueAsync(long newServiceID, int counterId, Queue request)
        {
            // Định nghĩa URL API
            string url = $"api/Queue/update-transfer-queue/{newServiceID}?counterId={counterId}";

            // Gửi yêu cầu PUT với dữ liệu body
            var response = await _httpClient!.PutAsJsonAsync(url, request);

            // Kiểm tra phản hồi
            if (response.IsSuccessStatusCode)
            {
                // Đọc kết quả trả về từ API
                return await response.Content.ReadFromJsonAsync<Queue>();
            }
            else
            {
                // Xử lý lỗi nếu có
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API call failed: {errorContent}");
            }
        }
        public async Task<bool> UpdateTransferQueue(long newServiceID, Queue request)
        {
            var result = await _httpClient!.PutAsJsonAsync($"api/Queue/update-transfer-queue/{newServiceID}", request);
            return result.IsSuccessStatusCode;

        }

        #endregion
        #region Service Table
        //Hiển thị dữ liệu Service theo ID được chọn
        public async Task<Service> GetServiceById(long id)
        {
            try
            {
                var result = await _httpClient!.GetFromJsonAsync<Service>($"api/Service/get-Service-by-id/{id}");
                return result!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null!;
            }
        }
        //Lọc dữ liệu Service để hiển thị ở trang đặt lịch
        public async Task<List<Service>> GetServiceList()
        {
            try
            {
                var list = await _httpClient!.GetFromJsonAsync<List<Service>>("api/Service/get-all-Service");
                return list ?? new List<Service>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return new List<Service>();
            }
        }
        //Xử lý dữ liệu trang Admin Dashboard
        public async Task<PagedList<Service>> GetServiceList(PagingParameters paging)
        {
            var queryStringParam = new Dictionary<string, string>
            {
                ["pageNumber"] = paging.PageNumber.ToString(),
                ["pageSize"] = paging.PageSize.ToString(),
                ["sortBy"] = paging.SortBy,
                ["sortOrder"] = paging.SortOrder,
                ["searchTerm"] = paging.SearchTerm!,
            };
            string url = QueryHelpers.AddQueryString("api/Service/get-all-Service-paging", queryStringParam!);
            var list = await _httpClient!.GetFromJsonAsync<PagedList<Service>>(url);
            return list!;
        }
        public async Task<bool> UpdateStatusService(long id, Service request)
        {
            var result = await _httpClient!.PutAsJsonAsync($"api/Service/update-Service/{id}", request);
            return result.IsSuccessStatusCode;

        }
        public async Task<bool> UpdateService(long id, ServiceDto request)
        {
            var result = await _httpClient!.PutAsJsonAsync($"api/Service/update-Service/{id}", request);
            return result.IsSuccessStatusCode;

        }
        public async Task<bool> CreateService(Service model)
        {
            var result = await _httpClient!.PostAsJsonAsync($"api/Service", model);
            return result.IsSuccessStatusCode;
        }

        public async Task<Queue> GetQueueByOrderNumberAndCounterIdAsync(int orderNumber, int counterId, int previousCounterId)
        {
            var result = await _httpClient!.GetFromJsonAsync<Queue>($"api/Queue/get-queue-by-ordernumber-and-counterId?orderNumber={orderNumber}&counterId={counterId}&previousCounterId={previousCounterId}");
            return result ?? new Queue(); // Trả về đối tượng Queue trống nếu không tìm thấy
        }

        #endregion
    }
}
