using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Entities
{
    public class ZnsTemplate
    {
        public int Id { get; set; }
        public string? TemplateID { get; set; }
        public string? TemplateName { get; set; }

        public int ZnsConfigId { get; set; }
        public ZnsConfig? ZnsConfig { get; set; }
    }
}
