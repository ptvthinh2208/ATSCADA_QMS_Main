using ATSCADA_Library.Helpers.ExportFile;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Entities
{
    [Table("Feedback")]
    public class Feedback
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [IgnoreExport] // Không export cột ID
        public int Id { get; set; }
        //public string? Name { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber {  get; set; }
        [IgnoreExport] // Không export cột ServiceID
        public long ServiceId {  get; set; }
        [IgnoreExport] // Không export cột NameService
        public string? NameService { get; set; } //Tên dịch vụ
        public string? ServiceDescription { get; set; } //Thông tin dịch vụ
        [IgnoreExport]
        public int CounterId { get; set; } //Id quầy
        public string? NameCounter { get; set; } //Tên quầy
        public int Rating { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
