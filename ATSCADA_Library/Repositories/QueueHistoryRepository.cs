using ATSCADA_Library.Data;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using ATSCADA_Library.Helpers.Sorting;
using ATSCADA_Library.Interfaces.Server;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Repositories
{
    public class QueueHistoryRepository : IQueueHistoryRepository
    {
        private readonly ATSCADADbContext _context;
        private readonly ZaloApiService _zaloApiService;
        public QueueHistoryRepository(ATSCADADbContext context, ZaloApiService zaloApiService)
        {
            _context = context;
            _zaloApiService = zaloApiService;
        }
        public async Task<PagedList<QueueHistory>> GetAllQueueHistoryAsync(PagingParameters paging)
        {
            var query = _context.QueueHistories.AsQueryable();
            // Start building the query by searching and optionally applying a date range filter
            if (paging.SearchBy != null)
            {
                query = query.Search(paging.SearchTerm!, paging.SearchBy);
            }
            else 
            {
                query = query.Search(paging.SearchTerm!, "PrintTime");
            }
            // Apply date range filter if provided
            if (paging.StartDate.HasValue && paging.EndDate.HasValue)
            {
                query = query.Where(f => f.PrintTime >= paging.StartDate.Value && f.PrintTime <= paging.EndDate.Value);
            }

            // Apply sorting (descending or ascending)
            if (paging.SortOrder == "desc")
            {
                var orderByDesc = string.Concat(paging.SortBy, " desc");
                query = query.Sort(orderByDesc);  // Assuming Sort extension method supports sorting by dynamic fields
            }
            else
            {
                query = query.Sort(paging.SortBy);  // Default sort order is ascending
            }
            // If PageSize is 0 or not provided, return all items without paging
            if (paging.PageSize == 0)
            {
                // Return all records without paging
                var fullList = await query.ToListAsync();
                return new PagedList<QueueHistory>(fullList, fullList.Count, 1, fullList.Count);
            }

            // Execute the query and paginate the results
            var list = await query.ToListAsync();
            return PagedList<QueueHistory>.ToPagedList(list, paging.PageNumber, paging.PageSize);
        }
        public async Task SendZnsForAllCustomers(List<QueueHistory> listModel, string templateName)
        {
            await _zaloApiService.SendTemplateMessageForAllAsync(listModel, templateName);
        }
        public async Task<Dictionary<string, List<int>>> GetChartDataAsync(PagingParameters paging, string status, long ServiceId, int counterId)
        {
            var query = _context.QueueHistories.AsQueryable();

            // Lọc theo trạng thái
            if (status == "TotalCompleted")
                query = query.Where(q => q.Status == "Completed");
            else if (status == "TotalCancel")
                query = query.Where(q => q.Status != "Completed");

            // Lọc theo ServiceID nếu được chọn
            if (ServiceId != 0)
                query = query.Where(q => q.ServiceId == ServiceId);
            // Lọc theo CounterID nếu được chọn
            if (counterId != 0)
                query = query.Where(q => q.CounterId == counterId);
            // Lọc theo khoảng thời gian
            query = query.Where(q => q.PrintTime.Date >= paging.StartDate.Value && q.PrintTime.Date <= paging.EndDate.Value);

            var stats = await query
                .GroupBy(q => q.PrintTime.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new Dictionary<string, List<int>>();
            foreach (var stat in stats)
            {
                if (!result.ContainsKey(stat.Date.ToString("yyyy-MM-dd")))
                    result[stat.Date.ToString("yyyy-MM-dd")] = new List<int>();

                result[stat.Date.ToString("yyyy-MM-dd")].Add(stat.Count);
            }

            return result;
        }
    }
}
