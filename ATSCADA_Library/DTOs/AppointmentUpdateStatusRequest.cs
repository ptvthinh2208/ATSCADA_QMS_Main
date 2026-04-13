using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.DTOs
{
    public class AppointmentUpdateStatusRequest
    {
        public string? Status { get; set; }
        public bool Verified { get; set; }
    }
}
