using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Interfaces.Client
{
    public interface IFeedbackApiClient
    {
        Task<PagedList<Feedback>> GetAllFeedback(PagingParameters paging, DateTime? startDate, DateTime? endDate);
        Task<bool> CreateFeedback(int rating, FeedbackDto dto);
    }
}
