using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.DTOs
{
    public class CounterDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Tên dịch vụ không được để trống")]
        public long ServiceId { get; set; }
        public int ApplicationUserId {  get; set; }
        [Required(ErrorMessage ="Tên quầy không được phép để trống!")]
        public string? NameCounter {  get; set; }
        //Code đóng vai trò làm tiếp đầu ngữ cho phiếu in 
        public string? Code { get; set; }
        public bool IsActive {  get; set; }
        
        public string? ServiceDescription { get; set; }
        public string? ServiceName { get; set; }
        public string? NameUser {  get; set; }
        public string? CreatedBy {  get; set; }
        public string? LastUpdatedBy { get; set; }
        public DateTime LastUpdated { get; set; }
        public int ModbusId { get; set; }
    }
}
