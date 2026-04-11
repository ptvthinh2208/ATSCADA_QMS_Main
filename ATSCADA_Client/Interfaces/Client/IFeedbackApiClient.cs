using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;

namespace ATSCADA_Client.Interfaces.Client
{
    public interface IFeedbackApiClient
    {
        Task<PagedList<Feedback>> GetAllFeedback(PagingParameters paging, DateTime? startDate, DateTime? endDate);
        Task<bool> CreateFeedback(int rating, FeedbackDto dto);
    }
}
