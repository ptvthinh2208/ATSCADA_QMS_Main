using ATSCADA_Library.Data;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using ATSCADA_Library.Helpers.Sorting;
using ATSCADA_Library.Interfaces.Server;
using Microsoft.EntityFrameworkCore;

namespace ATSCADA_Library.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ATSCADADbContext _context;

        public ReportRepository(ATSCADADbContext context)
        {
            _context = context;
        }

        public async Task<PagedList<Report>> GetAllReportsAsync(PagingParameters paging)
        {
            var query = _context.Reports.AsQueryable();
            // Start building the query by searching and optionally applying a date range filter
            if (paging.SearchBy != null)
            {
                query = query.Search(paging.SearchTerm!, paging.SearchBy);
            }
            else
            {
                query = query.Search(paging.SearchTerm!, "CreatedDate");
            }
            // Apply date range filter if provided
            if (paging.StartDate.HasValue && paging.EndDate.HasValue)
            {
                query = query.Where(f => f.CreatedDate >= paging.StartDate.Value && f.CreatedDate <= paging.EndDate.Value);
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
                return new PagedList<Report>(fullList, fullList.Count, 1, fullList.Count);
            }

            // Execute the query and paginate the results
            var list = await query.ToListAsync();
            return PagedList<Report>.ToPagedList(list, paging.PageNumber, paging.PageSize);
        }
        public async Task<PagedList<Report>> GetDetailedReportsAsync(PagingParameters paging)
        {
            var query = _context.Reports.AsQueryable();
            if (paging.StartDate.HasValue && paging.EndDate.HasValue)
            {
                query = query.Where(f => f.CreatedDate >= paging.StartDate.Value && f.CreatedDate <= paging.EndDate.Value);
            }

            // Lấy số liệu thống kê dịch vụ cho các báo cáo
            var serviceStats = await _context.QueueHistories
                .Where(q => q.PrintTime >= paging.StartDate && q.PrintTime <= paging.EndDate)
                .GroupBy(q => q.ServiceId)
                .Select(g => new Report
                {
                    ServiceId = g.Key,
                    TotalPrint = g.Count(),
                    TotalCompleted = g.Count(q => q.Status == "Completed"),
                    TotalMissed = g.Count(q => q.Status == "Missed"),
                    TotalCancel = g.Count(q => q.Status == "Waiting" || q.Status == "Cancel")
                })
                .ToListAsync();

            // Convert serviceStats to a dictionary for easy lookup by ServiceId
            var serviceStatsDictionary = serviceStats.ToDictionary(s => s.ServiceId);

            // Retrieve reports and map service statistics
            var reportsWithStats = await query
                .ToListAsync();  // Convert to list before further manipulation

            // Map service statistics to each report
            var result = reportsWithStats.Select(r => new Report
            {
                Id = r.Id,
                ServiceId = r.ServiceId,
                CreatedDate = r.CreatedDate,
                TotalPrint = serviceStatsDictionary.TryGetValue(r.ServiceId, out var stats) ? stats.TotalPrint : 0,
                TotalCompleted = serviceStatsDictionary.TryGetValue(r.ServiceId, out stats) ? stats.TotalCompleted : 0,
                TotalMissed = serviceStatsDictionary.TryGetValue(r.ServiceId, out stats) ? stats.TotalMissed : 0,
                TotalCancel = serviceStatsDictionary.TryGetValue(r.ServiceId, out stats) ? stats.TotalCancel : 0
            }).ToList();

            // Apply paging if necessary
            if (paging.PageSize == 0)
            {
                return new PagedList<Report>(serviceStats, serviceStats.Count, 1, serviceStats.Count);
            }

            return PagedList<Report>.ToPagedList(serviceStats, paging.PageNumber, paging.PageSize);
        }
        //public async Task<Dictionary<string, Dictionary<DateTime, int>>> GetDetailedReportsAsync(PagingParameters paging)
        //{
        //    var query = _context.Reports.AsQueryable();

        //    if (paging.StartDate.HasValue && paging.EndDate.HasValue)
        //    {
        //        query = query.Where(f => f.CreatedDate >= paging.StartDate.Value && f.CreatedDate <= paging.EndDate.Value);
        //    }

        //    // Lấy số liệu thống kê dịch vụ cho các báo cáo
        //    var serviceStats = await _context.QueueHistories
        //        .Where(q => q.PrintTime >= paging.StartDate && q.PrintTime <= paging.EndDate)
        //        .GroupBy(q => q.ServiceId)
        //        .Select(g => new
        //        {
        //            ServiceId = g.Key,
        //            TotalPrint = g.Count(),
        //            TotalCompleted = g.Count(q => q.Status == "Completed"),
        //            TotalMissed = g.Count(q => q.Status == "Missed"),
        //            TotalCancel = g.Count(q => q.Status == "Waiting" || q.Status == "Cancel")
        //        })
        //        .ToListAsync();

        //    // Chuyển đổi serviceStats thành từ điển để dễ dàng tra cứu
        //    var serviceStatsDictionary = serviceStats.ToDictionary(s => s.ServiceId);

        //    // Lấy báo cáo và ánh xạ thống kê dịch vụ
        //    var reportsWithStats = await query.ToListAsync();

        //    // Khởi tạo detailedReports cho các dịch vụ
        //    var detailedReports = new Dictionary<string, Dictionary<DateTime, int>>
        //        {
        //            { "TotalPrint", new Dictionary<DateTime, int>() },
        //            { "TotalCompleted", new Dictionary<DateTime, int>() },
        //            { "TotalMissed", new Dictionary<DateTime, int>() },
        //            { "TotalCancel", new Dictionary<DateTime, int>() }
        //        };

        //    foreach (var report in reportsWithStats)
        //    {

        //        if (!detailedReports["TotalPrint"].ContainsKey(report.CreatedDate))
        //            detailedReports["TotalPrint"][report.CreatedDate] = 0;
        //        if (!detailedReports["TotalCompleted"].ContainsKey(report.CreatedDate))
        //            detailedReports["TotalCompleted"][report.CreatedDate] = 0;
        //        if (!detailedReports["TotalMissed"].ContainsKey(report.CreatedDate))
        //            detailedReports["TotalMissed"][report.CreatedDate] = 0;
        //        if (!detailedReports["TotalCancel"].ContainsKey(report.CreatedDate))
        //            detailedReports["TotalCancel"][report.CreatedDate] = 0;

        //        detailedReports["TotalPrint"][report.CreatedDate] += stats.TotalPrint;
        //        detailedReports["TotalCompleted"][report.CreatedDate] += stats.TotalCompleted;
        //        detailedReports["TotalMissed"][report.CreatedDate] += stats.TotalMissed;
        //        detailedReports["TotalCancel"][report.CreatedDate] += stats.TotalCancel;

        //    }

        //    return detailedReports;
        //}

        public async Task<List<Report>> GetDetailsReportByIdAsync(int reportId)
        {
            var report = await _context.Reports.FirstOrDefaultAsync(x => x.Id == reportId);

            if (report == null)
            {
                throw new InvalidOperationException($"Không tìm thấy báo cáo với Id = {reportId}.");
            }

            var fromDate = report.CreatedDate.Date;
            var endDate = fromDate.AddHours(23).AddMinutes(59).AddSeconds(59);

            var reportsByCounter = await _context.QueueHistories
                .Where(q => q.PrintTime >= fromDate && q.PrintTime <= endDate)
                .GroupBy(q => q.ServiceId)
                .Select(g => new Report
                {
                    ServiceId = g.Key,
                    CreatedDate = fromDate,
                    TotalPrint = g.Count(),
                    TotalCompleted = g.Count(q => q.Status == "Completed"),
                    TotalMissed = g.Count(q => q.Status == "Missed"),
                    TotalCancel = g.Count(q => q.Status == "Waiting" || q.Status == "Cancel")
                })
                .ToListAsync();

            return reportsByCounter;
        }
    }
}
