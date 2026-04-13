using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;

namespace ATSCADA_Client.Response
{
    public class CallNextResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Queue Client { get; set; }
        public int CurrentNumber { get; set; }
    }
}
