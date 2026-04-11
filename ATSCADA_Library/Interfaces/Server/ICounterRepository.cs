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
    public interface ICounterRepository
    {
        Task<PagedList<Counter>> GetAllCounterAsync(PagingParameters paging);
        Task<List<Counter>> GetAllCountersRawAsync();
        Task<Counter> GetCounterByIdAsync(int id);
        Task<List<Counter>> GetCountersByIdsAsync(List<int> ids);
        Task<Counter> GetCounterByNameAsync(string counterName);
        Task<Counter> GetCounterByServiceIdAsync(long ServiceId);
        Task<List<Counter>> GetListCounterByServiceIdAsync(long ServiceId);
        Task<List<Queue>> GetQueuesByServiceIdAsync(long ServiceId);
        Task<Counter> CheckNameCounter(CounterDto counterDto);
        Task<Counter> UpdateAsync(Counter counter);
        Task<Counter> CreateNewCounterAsync(CounterDto dto);
        Task<CallNextResult> CallNextTicketAtomicAsync(int counterId, int serviceId);
    }
}
