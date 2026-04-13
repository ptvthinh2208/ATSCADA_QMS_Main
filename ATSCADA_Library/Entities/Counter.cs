using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Entities
{
    public class Counter
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Tên quầy không được để trống")]
        public string? Name { get; set; }

        //Code đóng vai trò làm tiếp đầu ngữ cho phiếu in 
        public string? Code {  get; set; }
        public int CurrentNumber { get; set; }
        public int TotalCount { get; set; }
        public bool IsActive { get; set; }
        //FK to Service   
        public long ServiceID { get; set; }
        public string? ServiceName { get; set; }
        public string? ServiceDescription { get; set; } 
        public string? CreatedBy {  get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? LastUpdatedBy { get; set; }
        public DateTime LastUpdatedDate { get; set; } = DateTime.Now;
        public TimeSpan AverageServiceTime { get; set; }
        // Navigation property
        public int ModbusId { get; set; }   // FK
        //public Modbus? Modbus { get; set; }


    }
}
