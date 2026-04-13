using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.DTOs
{
    public class ExportDataRequest<T>
    {
        public string? EntityType { get; set; }

        public IEnumerable<T>? Model { get; set; }
    }
    //public class ExportDataRequest
    //{
    //    public string? EntityType { get; set; } // Tên loại đối tượng (ví dụ: "ZaloSmsHistory")
    //    public List<object> Model { get; set; } // Chứa dữ liệu dưới dạng object
    //}

}
