using ATSCADA_Library.Entities;

namespace ATSCADA_Library.DTOs
{
    public class CallNextResult
    {
        public Queue? Queue { get; set; }
        public Counter? Counter { get; set; }
    }
}
