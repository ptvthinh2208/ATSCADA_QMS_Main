using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.DTOs
{
    public class FeedbackDto
    {
        public int Rating {  get; set; }
        public int OrderNumber {  get; set; }
        //public long ServiceId { get; set; }
        //public string? NameService { get; set; }
        public int CounterId {  get; set; }
        //public string? NameCounter {  get; set; }
    }
}
