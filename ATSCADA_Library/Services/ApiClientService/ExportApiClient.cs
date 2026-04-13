using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Interfaces.Client;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Services.ApiClientService
{
    public class ExportApiClient : IExportApiClient
    {
        public HttpClient? _httpClient;
        public ExportApiClient(HttpClient? httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<byte[]> ExportToExcel<T>(ExportDataRequest<T> request)
        {
            // Determine the type name for the API endpoint
            string typeName = typeof(T).Name;
            var response = await _httpClient!.PostAsJsonAsync($"api/Export/ExportToExcel/{typeName}", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }

            // Handle the error case as needed (e.g., throw an exception)
            throw new Exception("Export failed");
        }
        public async Task<byte[]> ExportToExcel(ExportDataRequest<Report> request)
        {
            var response = await _httpClient!.PostAsJsonAsync("api/Export/ExportToExcel/Report", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }

            // Handle the error case as needed (e.g., throw an exception, return null, etc.)
            throw new Exception("Export failed");
        }
        //public async Task<byte[]> ExportToExcel(ExportDataRequest<ReportDetailsByService> request)
        //{
        //    var response = await _httpClient!.PostAsJsonAsync("api/Export/ExportToExcel/ReportDetails", request);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        return await response.Content.ReadAsByteArrayAsync();
        //    }

        //    // Handle the error case as needed (e.g., throw an exception, return null, etc.)
        //    throw new Exception("Export failed");
        //}
        public async Task<byte[]> ExportToExcel(ExportDataRequest<Feedback> request)
        {
            var response = await _httpClient!.PostAsJsonAsync("api/Export/ExportToExcel/Feedback", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }

            // Handle the error case as needed (e.g., throw an exception, return null, etc.)
            throw new Exception("Export failed");
        }
        public async Task<byte[]> ExportToExcel(ExportDataRequest<QueueHistoryToExportData> request)
        {
            var response = await _httpClient!.PostAsJsonAsync("api/Export/ExportToExcel/QueueHistory", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }

            // Handle the error case as needed (e.g., throw an exception, return null, etc.)
            throw new Exception("Export failed");
        }
    }
}
