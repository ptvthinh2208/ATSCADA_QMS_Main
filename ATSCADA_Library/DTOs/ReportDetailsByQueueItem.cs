using ATSCADA_Library.Helpers.ExportFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.DTOs
{
    public class ReportDetailsByService
    {
        [IgnoreExport]
        public long ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public int TotalPrint { get; set; }
        public int TotalCompleted { get; set; } // Tổng số Completed
        public int TotalMissed { get; set; } // Tổng số Missed
        public int TotalCancel { get; set; } //Tổng số cancel
        
        public DateTime CreatedDate { get; set; }
        
    }
}
