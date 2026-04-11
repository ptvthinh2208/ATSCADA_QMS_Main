using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Interfaces.Client
{
    public interface IReportApiClient
    {
        Task<PagedList<Report>> GetAllReports(PagingParameters paging, DateTime? startDate, DateTime? endDate);
        Task<PagedList<Report>> GetDetailedReports(PagingParameters paging, DateTime? startDate, DateTime? endDate);
        //Task<Dictionary<string, Dictionary<DateTime, int>>> GetDetailedReportsAsync(PagingParameters paging, DateTime? startDate, DateTime? endDate);
        Task<List<Report>> GetDetailsReportById(int reportId);
    }
}
