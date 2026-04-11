using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Entities
{
    public class WorkShift
    {
        public int Id { get; set; }
        public string? NameWorkShift { get; set; }

        // Thời gian bắt đầu và kết thúc ca
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool isActive {  get; set; } //Đang mở hoặc khoá

    }
}
