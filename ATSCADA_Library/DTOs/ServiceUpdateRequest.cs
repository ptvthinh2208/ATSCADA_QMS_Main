namespace ATSCADA_Library.DTOs
{
    public class ServiceDto
    {
        public long Id { get; set; }
        public string? Name { get; set; } // Tên dịch vụ hoặc mã bàn
        
        public string? Description { get; set; }
        public bool IsActive { get; set; } // Bật hoặc tắt hàng đợi
        public string? LastUpdatedBy { get; set; }
        public DateTime LastUpdated {  get; set; }
        public int TotalPrint { get; set; }
    }
}
