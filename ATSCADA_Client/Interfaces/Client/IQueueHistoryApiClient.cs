using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Library.Entities;

namespace ATSCADA_Client.Interfaces.Client
{
    public interface IQueueHistoryApiClient
    {
        Task<PagedList<QueueHistory>> GetAllQueueHistories(PagingParameters paging, DateTime? startDate, DateTime? endDate);
        Task SendZnsForAllCustomers(List<QueueHistory> listModel, string templateName);
        Task<Dictionary<string, List<int>>> GetChartDataAsync(PagingParameters paging, DateTime? startDate, DateTime? endDate,
            string status, long ServiceId, int counterId);
    }
}
