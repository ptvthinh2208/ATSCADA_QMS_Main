using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Client.Response;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;

namespace ATSCADA_Client.Interfaces.Client
{
    public interface IQueueApiClient
    {
        Task<List<Queue>> GetListAllQueue();
        Task<PagedList<QueueHistory>> GetListQueueHistory(PagingParameters paging, DateTime? startDate, DateTime? endDate);
        Task<bool> CreateQueue(Appointment model);
        Task<List<Queue>> GetQueuesByCounterIdAsync(int counterId);
        Task<Queue> GetQueueByOrderNumberAndCounterIdAsync(int orderNumber, int counterId, int previousCounterId);
        Task<List<Queue>> GetQueuesByServiceId (long ServiceId);
        //Task<List<Queue>> GetQueuesByStatusesAndServiceIdAsync(List<string> statuses, long ServiceId);
        Task<Queue> GetQueuesByOrderNumberAndCounterIdAsync(int orderNumber, long ServiceId);
        Task<bool> UpdateQueueById(int id, Queue request);
        Task<bool> UpdateTransferQueue(long id, Queue request);
        //Cập nhật dữ liệu khi chuyển quầy
        Task<List<Queue?>> UpdateTransferQueueAsync(long newServiceID, int counterId, List<Queue> request);
        Task<Queue?> UpdateTransferQueueAsync(long newServiceID, int counterId, Queue request);

        Task<Queue?> CallNextNumberAsync(int serviceId, int counterId);
        //Service
        Task<List<Service>> GetServiceList();
        Task<PagedList<Service>> GetServiceList(PagingParameters paging);
        Task<Service> GetServiceById(long id);
        Task<bool> UpdateService(long id, ServiceDto request);
        Task<bool> UpdateStatusService(long id, Service request);
        Task<bool> CreateService(Service model);
        //MySql
        Task<QueueResponse> CallPrintService(long id);
    }
}
