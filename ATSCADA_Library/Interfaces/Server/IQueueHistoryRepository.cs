using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Interfaces.Server
{
    public interface IQueueHistoryRepository
    {
        Task<PagedList<QueueHistory>> GetAllQueueHistoryAsync(PagingParameters paging);
        Task SendZnsForAllCustomers(List<QueueHistory> listModel, string templateName);
        Task<Dictionary<string, List<int>>> GetChartDataAsync(PagingParameters paging,string status, long ServiceId, int counterId);
    }
}
