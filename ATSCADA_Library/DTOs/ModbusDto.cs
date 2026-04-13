using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.DTOs
{
    // Read DTO (trả về cho client)
    // Modbus DTOs
    public class ModbusDto
    {
        public int Id { get; set; }
        public string IpAddress { get; set; } = default!;
        public int Port { get; set; } = 502;
        public int SlaveId { get; set; } = 1;
        public int RegisterAddress { get; set; }
        //public int CounterId { get; set; }
    }


}
