using ATSCADA_Library.DTOs;
using ATSCADA_Library.DTOs.Response;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Interfaces.Client
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
        Task<Queue?> UpdateTransferQueueAsync(long newServiceID, int counterId, Queue request);


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
