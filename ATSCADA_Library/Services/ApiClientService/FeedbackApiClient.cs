using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using ATSCADA_Library.Interfaces.Client;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Services.ApiClientService
{
    public class FeedbackApiClient : IFeedbackApiClient
    {
        public HttpClient? _httpClient;
        public FeedbackApiClient(HttpClient? httpClient)
        {
            _httpClient = httpClient;
        }
        public Task<bool> CreateFeedback(int rating, FeedbackDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedList<Feedback>> GetAllFeedback(PagingParameters paging, DateTime? startDate, DateTime? endDate)
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
            // Add date range to the query parameters if provided
            if (startDate.HasValue)
                queryStringParam["startDate"] = startDate.Value.ToString("yyyy-MM-dd");
            if (endDate.HasValue)
                queryStringParam["endDate"] = endDate.Value.ToString("yyyy-MM-dd");
            string url = QueryHelpers.AddQueryString("api/Feedback/get-all-feedback", queryStringParam!);
            var list = await _httpClient!.GetFromJsonAsync<PagedList<Feedback>>(url);
            return list!;
        }
    }
}
