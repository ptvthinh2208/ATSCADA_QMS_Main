using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.DTOs
{
    public class PrintRequest
    {
        public string? FullName { get; set; }
        public string? TicketNumber { get; set; }
        public string? ServiceName {  get; set; }
        public string? AppointmentTime { get; set; }
        public string? PrinterIP { get; set; }
        public string? VehicleNumber {  get; set; }
    }
}
