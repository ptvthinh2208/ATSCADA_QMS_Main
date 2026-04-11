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
    public class ReportApiClient : IReportApiClient
    {
        public HttpClient? _httpClient { get; set; }
        public ReportApiClient(HttpClient? httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<PagedList<Report>> GetAllReports(PagingParameters paging, DateTime? startDate, DateTime? endDate)
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
            string url = QueryHelpers.AddQueryString("api/report/get-all-report-paging", queryStringParam!);
            var result = await _httpClient!.GetFromJsonAsync<PagedList<Report>>(url);
            return result!;
        }
        public async Task<PagedList<Report>> GetDetailedReports(PagingParameters paging, DateTime? startDate, DateTime? endDate)
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
            string url = QueryHelpers.AddQueryString("api/report/get-all-detailed-report-paging", queryStringParam!);
            var result = await _httpClient!.GetFromJsonAsync<PagedList<Report>>(url);
            return result!;
        }
        public async Task<List<Report>> GetDetailsReportById(int reportId)
        {
            var result = await _httpClient!.GetFromJsonAsync<List<Report>>($"api/report/get-details-report-by-id/{reportId}");
            return result!;
        }

        
    }
}
