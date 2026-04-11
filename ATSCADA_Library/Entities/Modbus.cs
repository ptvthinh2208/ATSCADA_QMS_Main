namespace ATSCADA_Library.Entities
{
    public class Modbus
    {
        public int Id { get; set; }

        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; } = 502;
        public int SlaveId { get; set; } = 1;
        public int RegisterAddress { get; set; }
        
    }
}
