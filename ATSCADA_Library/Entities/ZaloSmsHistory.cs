using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ATSCADA_Library.Entities
{
    public class ZaloSmsHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id {  get; set; }
        public DateTime? SendTime { get; set; }
        public string? PhoneNumber { get; set; }
        public string? NameService { get; set; }
        public string? ZaloType {  get; set; }
        public bool Status {  get; set; }

    }
}
