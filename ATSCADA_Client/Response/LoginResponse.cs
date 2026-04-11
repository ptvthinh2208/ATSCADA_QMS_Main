using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Client.Response
{
    public class LoginResponse
    {
        public bool Successful { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public long RoleUser {  get; set; } //1 : Admin ; 2 : Manager
        public int CounterId {  get; set; }
    }
}
