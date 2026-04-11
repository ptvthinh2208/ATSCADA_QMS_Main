using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using ATSCADA_Library.Interfaces.Server;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Services.ApiService
{
    public class CounterService
    {
        private readonly ICounterRepository _counterRepository;

        public CounterService(ICounterRepository counterRepository)
        {
            _counterRepository = counterRepository;
        }
        public async Task<List<Counter>> GetListCounterByServiceIdAsync(long ServiceId)
        {
            if (ServiceId <= 0)
            {
                throw new ArgumentException("Invalid ID provided", nameof(ServiceId));
            }

            var counter = await _counterRepository.GetListCounterByServiceIdAsync(ServiceId);
            if (counter == null)
            {
                throw new KeyNotFoundException($"Counter with ID {ServiceId} not found.");
            }
            return counter;
        }
        public async Task<Counter> GetCounterByServiceIdAsync(long ServiceId)
        {
            if (ServiceId <= 0)
            {
                throw new ArgumentException("Invalid ID provided", nameof(ServiceId));
            }

            var counter = await _counterRepository.GetCounterByServiceIdAsync(ServiceId);
            if (counter == null)
            {
                throw new KeyNotFoundException($"Counter with ID {ServiceId} not found.");
            }

            if (!counter.IsActive)
            {
                throw new InvalidOperationException("Counter is not active.");
            }

            return counter;
        }
        public async Task<Counter> GetCounterByIdAsync(int counterId)
        {

            var counter = await _counterRepository.GetCounterByIdAsync(counterId);
            if (counter == null)
                throw new InvalidOperationException($"Counter with id '{counterId}' not found.");
            return counter;
        }
        public async Task<List<Counter>> GetCountersByIdsAsync(List<int> ids)
        {
            return await _counterRepository.GetCountersByIdsAsync(ids);
        }

        //Hàm update Total Count khi có record mới được tạo ở bảng Queue
        //public async Task<Counter> UpdateTotalCountAsync(long ServiceId)
        //{
        //    var counter = await GetCounterByServiceIdAsync(ServiceId);
        //    var countDataListQueue = await _counterRepository.GetQueuesByServiceIdAsync(ServiceId);

        //    counter.TotalCount = countDataListQueue.Count;
        //    //if(counter.CurrentNumber == 0)
        //    //{
        //    //    counter.CurrentNumber = 1;
        //    //}
        //    return await _counterRepository.UpdateAsync(counter);
        //}
        //Update số tổng phiếu in khi có 1 khách hàng đăng ký lịch
        public async Task<Counter> UpdateTotalCountAsync(int counterId)
        {
            var counter = await _counterRepository.GetCounterByIdAsync(counterId);

            counter.TotalCount += 1;

            return await _counterRepository.UpdateAsync(counter);
        }
        public async Task<Counter> UpdateCounterByIdAsync(CounterDto counterDto)
        {
            var counter = await _counterRepository.GetCounterByIdAsync(counterDto.Id);
            // Only check for existing name if the name is being changed
            if (counter.Name != counterDto.NameCounter)
            {
                var existingCounter = await _counterRepository.CheckNameCounter(counterDto);
                if (existingCounter != null)
                {
                    throw new Exception("A counter with this name already exists.");
                }
            }
            counter.Name = counterDto.NameCounter;
            counter.Code = counterDto.Code;
            counter.LastUpdatedBy = counterDto.LastUpdatedBy;
            counter.LastUpdatedDate = counterDto.LastUpdated;
            counter.ServiceID = counterDto.ServiceId;
            counter.ServiceName = counterDto.ServiceName;
            counter.ServiceDescription = counterDto.ServiceDescription;
            counter.IsActive = counterDto.IsActive;
            return await _counterRepository.UpdateAsync(counter);
        }
        public async Task<Counter> UpdateAvgCounterByIdAsync(int counterId, TimeSpan averageServiceTime)
        {
            var counter = await _counterRepository.GetCounterByIdAsync(counterId);
            counter.AverageServiceTime = averageServiceTime;
            return await _counterRepository.UpdateAsync(counter);
        }
        //public async Task<Counter> UpdateCounterByServiceIdAsync(long ServiceId, int currentOrderNumber)
        //{
        //    var counter = await GetCounterByServiceIdAsync(ServiceId);

        //    if(currentOrderNumber <= counter.TotalCount)
        //    {
        //        counter.CurrentNumber = currentOrderNumber;
        //    }
        //    return await _counterRepository.UpdateAsync(counter);
        //}
        public async Task<Counter> UpdateCounterNumberByIdAsync(int counterId, int currentOrderNumber)
        {
            var counter = await GetCounterByIdAsync(counterId);


            counter.CurrentNumber = currentOrderNumber;

            return await _counterRepository.UpdateAsync(counter);
        }
        public async Task<PagedList<Counter>> GetAllCounterAsync(PagingParameters paging)
        {
            return await _counterRepository.GetAllCounterAsync(paging);
        }
        public async Task<Counter> CreateNewCounter(CounterDto dto)
        {
            return await _counterRepository.CreateNewCounterAsync(dto);
        }
        public async Task<CallNextResult> CallNextTicketAtomicAsync(int counterId, int serviceId)
        {
            return await _counterRepository.CallNextTicketAtomicAsync(counterId, serviceId);
        }
    }
}
