using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.DTOs
{
    public class ClientOnQueueRequest
    {
        public int OrderNumber { get; set; }
        public long ServiceId { get; set; }
    }
}
