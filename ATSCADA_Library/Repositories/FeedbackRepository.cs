using ATSCADA_Library.Data;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using ATSCADA_Library.Helpers.Sorting;
using ATSCADA_Library.Interfaces.Server;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly ATSCADADbContext _context;
        private readonly IQueueRepository _queueRepository;
        public FeedbackRepository(ATSCADADbContext context, IQueueRepository queueRepository)
        {
            _context = context;
            _queueRepository = queueRepository;
        }
        public async Task<PagedList<Feedback>> GetAllFeedbacksAsync(PagingParameters paging)
        {
            var query = _context.Feedbacks.AsQueryable();
            // Start building the query by searching and optionally applying a date range filter
            if (paging.SearchBy != null)
            {
                query = query.Search(paging.SearchTerm!, paging.SearchBy);
            }
            else
            {
                query = query.Search(paging.SearchTerm!, "ServiceDescription");
            }
            // Apply date range filter if provided
            if (paging.StartDate.HasValue && paging.EndDate.HasValue)
            {
                query = query.Where(f => f.CreatedDate >= paging.StartDate.Value && f.CreatedDate <= paging.EndDate.Value);
            }

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
                return new PagedList<Feedback>(fullList, fullList.Count, 1, fullList.Count);
            }

            // Execute the query and paginate the results
            var list = await query.ToListAsync();
            return PagedList<Feedback>.ToPagedList(list, paging.PageNumber, paging.PageSize);
        }
        public async Task<Feedback> CreateFeedbackAsync(FeedbackDto dto)
        {
            try
            {
                var infoClient = await _queueRepository.GetQueueByOrderNumberAndCounterId(dto.OrderNumber, dto.CounterId);
                if (infoClient == null)
                {
                    throw new Exception("Không tìm thấy thông tin khách hàng!");
                }
                // Lấy thông tin Counter
                var infoCounter = await _context.Counters.FindAsync(dto.CounterId);
                if (infoCounter == null)
                {
                    throw new Exception("Không tìm thấy thông tin counter!");
                }
                // Tìm feedback đã tồn tại của khách hàng trong ngày
                var existingFeedback = await _context.Feedbacks
                    .Where(f => f.PhoneNumber == infoClient.PhoneNumber && f.CreatedDate.Date == DateTime.Now.Date)
                    .OrderByDescending(f => f.Rating) // Sắp xếp giảm dần theo rating
                    .FirstOrDefaultAsync();

                // Nếu feedback trong ngày đã tồn tại và rating mới cao hơn, cập nhật rating
                if (existingFeedback != null)
                {
                    if (dto.Rating > existingFeedback.Rating)
                    {
                        existingFeedback.Rating = dto.Rating;
                        await _context.SaveChangesAsync();
                    }
                    return existingFeedback;
                }
                // Nếu chưa có feedback trong ngày, tạo mới
                var feedback = new Feedback
                {
                    Rating = dto.Rating,
                    ServiceId = infoClient.ServiceId,
                    FullName = infoClient.FullName,
                    PhoneNumber = infoClient.PhoneNumber,
                    NameService = infoClient.NameService,
                    ServiceDescription = infoClient.DescriptionService,
                    CounterId = infoClient.CounterId,
                    NameCounter = infoCounter!.Name,
                    CreatedDate = DateTime.Now,
                };
                _context.Add(feedback);

                await _context.SaveChangesAsync();
                return feedback;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            } 

        }
        public Task<Feedback> UpdateFeedbackAsync(Feedback model)
        {
            throw new NotImplementedException();
        }
    }
}
