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
    public class QueueHistoryApiClient : IQueueHistoryApiClient
    {
        public HttpClient? _httpClient { get; set; }
        public QueueHistoryApiClient(HttpClient? httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<PagedList<QueueHistory>> GetAllQueueHistories(PagingParameters paging, DateTime? startDate, DateTime? endDate)
        {
            var queryStringParam = new Dictionary<string, string>
            {
                ["pageNumber"] = paging.PageNumber.ToString(),
                ["pageSize"] = paging.PageSize.ToString(),
                ["sortBy"] = paging.SortBy,
                ["sortOrder"] = paging.SortOrder,
                ["searchTerm"] = paging.SearchTerm!,
                ["searchBy"] = paging.SearchBy!,
            };
            if (startDate.HasValue)
                queryStringParam["startDate"] = startDate.Value.ToString("yyyy-MM-dd");
            if (endDate.HasValue)
                queryStringParam["endDate"] = endDate.Value.ToString("yyyy-MM-dd");
            string url = QueryHelpers.AddQueryString("api/queuehistory/get-all-queue-history-paging", queryStringParam!);
            var result = await _httpClient!.GetFromJsonAsync<PagedList<QueueHistory>>(url);
            return result!;
        }
        public async Task<Dictionary<string, List<int>>> GetChartDataAsync(PagingParameters paging, DateTime? startDate, DateTime? endDate,
            string status, long ServiceId, int counterId)
        {
            var queryStringParam = new Dictionary<string, string>
            {
                ["pageNumber"] = paging.PageNumber.ToString(),
                ["pageSize"] = paging.PageSize.ToString(),
                ["sortBy"] = paging.SortBy,
                ["sortOrder"] = paging.SortOrder,
                ["searchTerm"] = paging.SearchTerm!,
                ["searchBy"] = paging.SearchBy!,
                ["status"] = status,
                ["ServiceId"] = ServiceId.ToString(),
                ["counterId"] = counterId.ToString()
            };
            if (startDate.HasValue)
                queryStringParam["startDate"] = startDate.Value.ToString("yyyy-MM-dd");
            if (endDate.HasValue)
                queryStringParam["endDate"] = endDate.Value.ToString("yyyy-MM-dd");
            // Build URL with query parameters
            string url = QueryHelpers.AddQueryString("api/QueueHistory/get-data-chart", queryStringParam);

            // Make the API call and return the response
            var response = await _httpClient!.GetFromJsonAsync<Dictionary<string, List<int>>>(url);
            return response ?? new Dictionary<string, List<int>>();
        }
        public async Task SendZnsForAllCustomers(List<QueueHistory> listModel, string templateName)
        {
            var request = new SendZnsRequest
            {
                ListModel = listModel,
                TemplateName = templateName
            };

            var response = await _httpClient!.PostAsJsonAsync("api/queuehistory/send-messages-for-customers", request);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Messages sent successfully.");
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error sending messages: {errorMessage}");
            }
        }
    }
}
