using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.DTOs
{
    public class CounterCallNextRequest
    {
        public string? CurrentTicket { get; set; }
        public string? NextTicket { get; set; }
    }
}
