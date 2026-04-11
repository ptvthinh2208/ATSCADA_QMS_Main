using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Entities
{
    public class ZnsConfig
    {
        public int Id { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? AppID { get; set; }
        public string? SecretKey { get; set; }

        public ICollection<ZnsTemplate>? Templates { get; set; }
    }
}
