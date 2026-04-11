using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Entities
{
    [Table("Queues")]
    public class Queue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int OrderNumber { get; set; }
        public string? NameService { get; set; }
        public string? DescriptionService { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        //public string? VehicleNumber { get; set; }
        public string? IdentificationNumber { get; set; }
        public DateTime AppointmentDate { get; set; }
        public DateTime AppointmentTime { get; set; }
        public string? Message { get; set; }
        public string? Status { get; set; } = "Waiting";
        public bool Verified { get; set; } = true; // Trạng thái xác minh
        //FK to Service
        public long ServiceId { get; set; }
        public Service? Service { get; set; }
        //FK to Counter
        public int CounterId { get; set; }
        public int OriginalCounterId { get; set; } //Quầy gốc ban đầu khi đã chuyển quầy, mặc định khi tạo = Counter Id

        //FK to Appointment
        public long AppointmentId { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
        public DateTime PrintTime { get; set; } = DateTime.Now; // Ngày tạo

        public bool isNotified { get; set; } = false;
        public bool Priority { get; set; } = false;
        public DateTime LastTimeUpdated { get; set; }

        
    }
}
