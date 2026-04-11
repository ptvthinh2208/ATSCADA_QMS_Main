namespace ATSCADA_Library.DTOs.Response
{
    public class LoginResponse
    {
        public bool Successful { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public long RoleUser { get; set; } //1 : Admin ; 2 : Manager
        public int CounterId { get; set; }
    }
}
