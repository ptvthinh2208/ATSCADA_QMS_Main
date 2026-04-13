using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Interfaces.Client
{
    public interface IQueueHistoryApiClient
    {
        Task<PagedList<QueueHistory>> GetAllQueueHistories(PagingParameters paging, DateTime? startDate, DateTime? endDate);
        Task SendZnsForAllCustomers(List<QueueHistory> listModel, string templateName);
        Task<Dictionary<string, List<int>>> GetChartDataAsync(PagingParameters paging, DateTime? startDate, DateTime? endDate,
            string status, long ServiceId, int counterId);
    }
}
