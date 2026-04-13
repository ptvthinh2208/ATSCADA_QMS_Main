using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Interfaces.Server
{
    public interface IReportRepository
    {
        Task<PagedList<Report>> GetAllReportsAsync(PagingParameters paging);
        Task<List<Report>> GetDetailsReportByIdAsync(int reportId);
        Task<PagedList<Report>> GetDetailedReportsAsync(PagingParameters paging);
        //Task<Dictionary<string, Dictionary<DateTime, int>>> GetDetailedReportsAsync(PagingParameters paging);
    }
}
