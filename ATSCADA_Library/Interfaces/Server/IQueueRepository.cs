using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Interfaces.Server
{
    public interface IQueueRepository
    {
        Task<List<Queue>> GetAllQueueAsync();
        Task<List<Queue>> GetAllQueueByServiceId(long id);
        Task<Queue> GetQueueById(int id);
        Task<List<Queue>> GetQueuesByStatusesAsync(List<string> statuses,long ServiceId);
        Task<List<Queue>> GetQueuesByCounterIdAsync(int counterId);
        //Task<Queue> GetQueueByOrderNumberAndCounterNameAsync(int orderNumber, string counterName);
        Task<Queue> GetQueueByOrderNumberAndCounterId(int orderNumber, int counterId);
        Task<Queue> GetQueueByOrderNumberAndCounterId(int orderNumber, int counterId, int previousCounterId);
        Task<Queue> CreateQueueAsync(QueueDto dto);
        Task<Queue> UpdateQueueAsync(Queue model);
        Task<List<Queue>> UpdateTransferQueueAsync(List<Queue> oldModel, long newServiceId, int counterId);
        Task<Queue> UpdateTransferQueueAsync(Queue oldModel, long newServiceId, int counterId);

        //Service
        Task<List<Service>> GetServiceList();
        Task<PagedList<Service>> GetServiceList(PagingParameters paging);
        Task<Service> GetServiceById(long id);
        Task<Service> UpdateService(Service model);
        Task<Service> CreateServiceAsync(Service model);
        Task<Service> CheckNameService(ServiceDto dto);
        Task UpdateTotalTicketPrintDirectly(long ServiceId);
    }
}
