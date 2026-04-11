using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Entities
{
    [Table("Services")]
    public class Service
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required(ErrorMessage = "Tên dịch vụ không được để trống")]
        public string? Name { get; set; } // Tên dịch vụ hoặc mã bàn
        [Required(ErrorMessage = "Thông tin dịch vụ không được để trống")]
        public string? Description { get; set; }
        public int TotalTicketPrint { get; set; } //Tổng phiếu đã in 
        public bool IsActive { get; set; } // Bật hoặc tắt hàng đợi
        //FK to Counter table
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? LastUpdatedBy { get; set; }
        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;
    }
}
