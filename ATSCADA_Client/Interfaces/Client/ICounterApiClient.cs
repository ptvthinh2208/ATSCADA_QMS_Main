using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;

namespace ATSCADA_Client.Interfaces.Client
{
    public interface ICounterApiClient
    {
        Task<PagedList<Counter>> GetAllCounter(PagingParameters paging);
        Task<Counter> GetCounterByServiceId(long ServiceId);
        Task<Counter> GetCounterById(int counterId);
        Task<List<Counter>> GetCountersByIds(IEnumerable<int> counterIds);
        Task<List<Counter>> GetListCounterByServiceId(long ServiceId);
        //Task<bool> UpdateCallNextNumber(long ServiceId, int orderNumber);
        Task<Counter?> UpdateCallNextNumberAsync(int counterId, int currentOrderNumber);
        Task<bool> CreateTextCallCurrentNumber(string counterName,string code, int orderNumber, string nameClient, string vehicleNumber);
        Task<Counter> CreateNewCounter(CounterDto dto);
        Task<Counter> UpdateCounter(CounterDto dto);
        Task<bool> UpdateCounterAvgTimeAsync(int counterId, string averageServiceTime);

    }
}
