using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Library.Entities;

namespace ATSCADA_Client.Interfaces.Client
{
    public interface IReportApiClient
    {
        Task<PagedList<Report>> GetAllReports(PagingParameters paging, DateTime? startDate, DateTime? endDate);
        Task<PagedList<Report>> GetDetailedReports(PagingParameters paging, DateTime? startDate, DateTime? endDate);
        //Task<Dictionary<string, Dictionary<DateTime, int>>> GetDetailedReportsAsync(PagingParameters paging, DateTime? startDate, DateTime? endDate);
        Task<List<Report>> GetDetailsReportById(int reportId);
    }
}
