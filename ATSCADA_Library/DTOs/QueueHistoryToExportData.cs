using ATSCADA_Library.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.DTOs
{
    public class QueueHistoryToExportData
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? DescriptionService { get; set; }
        public string? CounterName { get; set; }
        public string? OrderNumber { get; set; }
        public string? Status { get; set; } 
        public DateTime PrintTime { get; set; }
        public DateTime? LastTimeUpdated { get; set; }

    }
}
