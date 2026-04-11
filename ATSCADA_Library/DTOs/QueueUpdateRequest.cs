using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.DTOs
{
    public class QueueUpdateRequest
    {
        public bool Priority { get; set; }
        public bool isNotified { get; set; }
        public string? Status { get; set; }
        public int CounterId { get; set; }
        public int OriginalCounterId { get; set; }
    }
}
