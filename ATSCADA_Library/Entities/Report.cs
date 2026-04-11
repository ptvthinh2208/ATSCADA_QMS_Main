using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATSCADA_Library.Helpers.ExportFile;

namespace ATSCADA_Library.Entities
{
    [Table("Reports")]
    public class Report
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        [IgnoreExport]
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalPrint {  get; set; }
        public int TotalCompleted { get; set; } // Tổng số Completed
        public int TotalMissed { get; set; } // Tổng số Missed
        public int TotalCancel { get; set; } //Tổng số cancel
        [IgnoreExport]
        public long ServiceId {  get; set; }
    }

}
