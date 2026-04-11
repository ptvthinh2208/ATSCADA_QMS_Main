using ATSCADA_Library.Data;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using ATSCADA_Library.Helpers.Sorting;
using ATSCADA_Library.Interfaces.Server;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace ATSCADA_Library.Repositories
{
    public class CounterRepository : ICounterRepository
    {
        private readonly ATSCADADbContext _context;
        public CounterRepository(ATSCADADbContext context)
        {
            _context = context;
        }

        public async Task<Counter> GetCounterByServiceIdAsync(long ServiceId)
        {
            var result = await _context.Counters.Where(x => x.ServiceID == ServiceId).FirstOrDefaultAsync();
            return result!;
        }
        public async Task<List<Counter>> GetListCounterByServiceIdAsync(long ServiceId)
        {
            var result = await _context.Counters.Where(x => x.ServiceID == ServiceId && x.IsActive == true).ToListAsync();
            return result!;
        }
        public async Task<Counter> CheckNameCounter(CounterDto counterDto)
        {
            var result = await _context.Counters.Where(x => x.Name == counterDto.NameCounter).FirstOrDefaultAsync();
            return result!;
        }
        public async Task<Counter> GetCounterByNameAsync(string counterName)
        {
            if (string.IsNullOrWhiteSpace(counterName))
                throw new ArgumentException("Counter name cannot be null or empty.", nameof(counterName));

            // Find the counter by name
            var counter = await _context.Counters
                .FirstOrDefaultAsync(c => c.Name == counterName);

            if (counter == null)
                throw new InvalidOperationException($"Counter with name '{counterName}' not found.");

            return counter;
        }
        public async Task<List<Queue>> GetQueuesByServiceIdAsync(long ServiceId)
        {
            return await _context.Queues.Where(x => x.ServiceId == ServiceId).ToListAsync();
        }
        public async Task<List<Counter>> GetAllCountersRawAsync()
        {
            return await _context.Counters.ToListAsync();
        }
        public async Task<PagedList<Counter>> GetAllCounterAsync(PagingParameters paging)
        {
            // Start building the query by searching and optionally applying a date range filter
            var query = _context.Counters
                                .Search(paging.SearchTerm!, "Name")
                                .AsQueryable();
            // Apply sorting (descending or ascending)
            if (paging.SortOrder == "desc")
            {
                var orderByDesc = string.Concat(paging.SortBy, " desc");
                query = query.Sort(orderByDesc);  // Assuming Sort extension method supports sorting by dynamic fields
            }
            else
            {
                query = query.Sort(paging.SortBy);  // Default sort order is ascending
            }

            // If PageSize is 0 or not provided, return all items without paging
            if (paging.PageSize == 0)
            {
                // Return all records without paging
                var fullList = await query.ToListAsync();
                return new PagedList<Counter>(fullList, fullList.Count, 1, fullList.Count);
            }

            // Execute the query and paginate the results
            var list = await query.ToListAsync();
            return PagedList<Counter>.ToPagedList(list, paging.PageNumber, paging.PageSize);
        }
        public async Task<Counter> UpdateAsync(Counter counter)
        {
            
            _context.Counters.Update(counter);
            await _context.SaveChangesAsync();
            return counter;
        }
        public async Task<Counter> CreateNewCounterAsync(CounterDto dto)
        {

            var existingCounter = await _context.Counters
                                             .FirstOrDefaultAsync(u => u.Name == dto.NameCounter);
            if (existingCounter != null)
            {
                return null!;
            }
            var counter = new Counter
            {
                Name = dto.NameCounter,
                Code = dto.Code,
                IsActive = true,
                ServiceName = dto.ServiceName,
                ServiceDescription = dto.ServiceDescription,
                ServiceID = dto.ServiceId,
                CreatedBy = dto.CreatedBy,
                ModbusId = dto.ModbusId,
            };
            await _context.Counters.AddAsync(counter);
            await _context.SaveChangesAsync();
            return counter;
        }

        public async Task<Counter> GetCounterByIdAsync(int id)
        {
            var counter = await _context.Counters.FindAsync(id);
            if (counter != null)
            {
                return counter;
            }
            else return null!;
        }
        //public async Task<Counter> GetNextNumber(int counterId)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    var counter = _context.Counters.Where(c => c.Id == counterId).FirstOrDefault();
        //    // Lấy số kế tiếp trong transaction
        //    var listClientsWaiting = await _context.Queues
        //        .Where(q => q.Status == "Waiting" && q.DescriptionService == counter!.ServiceID.ToString())
        //        .OrderBy(q => q.PrintTime)
        //        .FirstOrDefaultAsync();
        //    if (listClientsWaiting == null)
        //        return null;
        //}
        public async Task<CallNextResult> CallNextTicketAtomicAsync(int counterId, int serviceId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var today = DateTime.Today.Date;
                var nextClient = await _context.Queues
                    .FromSqlRaw(@"
                SELECT * FROM Queues 
                WHERE Status = 'Waiting'
                  AND ServiceId = {1}
                  AND CounterId = 0
                ORDER BY OrderNumber
                LIMIT 1
                FOR UPDATE
            ", today, serviceId)
                    .FirstOrDefaultAsync();

                if (nextClient == null)
                {
                    await transaction.RollbackAsync();
                    return new CallNextResult { Queue = null, Counter = null };
                }

                // Kiểm tra xem record có bị quầy khác lấy không
                if (nextClient.CounterId != 0)
                {
                    await transaction.RollbackAsync();
                    return new CallNextResult { Queue = null, Counter = null };
                }

                nextClient.CounterId = counterId;
                nextClient.Status = "Processing";
                nextClient.LastTimeUpdated = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Lấy thông tin Counter để đồng bộ LED
                var counter = await _context.Counters.FindAsync(counterId);

                return new CallNextResult 
                { 
                    Queue = nextClient, 
                    Counter = counter 
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Lỗi khi gọi khách tiếp theo: " + ex.Message, ex);
            }
        }

        public async Task<List<Counter>> GetCountersByIdsAsync(List<int> ids)
        {
            return await _context.Counters
                .Where(c => ids.Contains(c.Id) && c.IsActive)
                .Select(c => new Counter
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    IsActive = c.IsActive
                })
                .ToListAsync();
        }

    }
}
