using ATSCADA_Library.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.DTOs
{
    public class SendZnsRequest
    {
        public List<QueueHistory> ListModel { get; set; } = new();
        public string TemplateName { get; set; } = string.Empty;
    }
}
