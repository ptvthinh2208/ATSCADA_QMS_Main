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
    public interface IFeedbackRepository
    {
        Task<PagedList<Feedback>> GetAllFeedbacksAsync(PagingParameters paging);
        Task<Feedback> CreateFeedbackAsync(FeedbackDto dto);
        Task<Feedback> UpdateFeedbackAsync(Feedback model);
    }
}
