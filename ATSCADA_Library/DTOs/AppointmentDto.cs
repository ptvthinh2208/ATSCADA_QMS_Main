namespace ATSCADA_Library.DTOs
{
    public class QueueDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? IdentificationNumber { get; set; }
        public DateTime AppointmentDate { get; set; } = DateTime.Now;
        //public TimeOnly AppointmentTime { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

        public int Quantity { get; set; }
        public string? Message { get; set; }
        public long ServiceId { get; set; } = 1; //Bên công ty không yêu cầu dịch vụ mặc định dịch vụ 1
        public bool isPrinting { get; set; } = true;
        public string? PrinterIP { get; set; }

    }

}
