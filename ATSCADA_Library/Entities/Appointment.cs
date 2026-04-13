using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Entities
{
    [Table("Appointments")]
    public class Appointment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime AppointmentDate { get; set; } = DateTime.Now;
        public TimeSpan AppointmentTime { get; set; }
        public string? Message { get; set; }

        // Trạng thái Appointment: "Unverified", "Waiting", "Completed", "Canceled"
        public string? Status { get; set; } = "Unverified";
        public bool Verified { get; set; } = false; // Trạng thái xác minh
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Foreign key đến Service
        public long ServiceId { get; set; } // Đảm bảo kiểu dữ liệu khớp với khóa chính của Service
        [NotMapped]
        public bool isPrinting { get; set; }
        [NotMapped]
        public string? PrinterIP { get; set; }

    }
}
