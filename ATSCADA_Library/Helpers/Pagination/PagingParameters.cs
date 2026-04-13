using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Helpers.Pagination
{
    public class PagingParameters
    {
        const int maxPageSize = 50;
        public int PageNumber { get; set; } = 1;
        private int _pageSize = 0;
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }
        public string SortBy { get; set; } = "Id";  // Default sort column
        public string SortOrder { get; set; } = "asc";  // "asc" or "desc"
        public string? SearchTerm { get; set; }
        public string? SearchBy{ get; set; }


        // Add date range properties
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
