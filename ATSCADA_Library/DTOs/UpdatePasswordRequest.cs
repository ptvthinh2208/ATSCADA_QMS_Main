using ATSCADA_Library.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.DTOs
{
    public class UpdatePasswordRequest
    {
        public ChangePasswordModel PasswordModel { get; set; }
        public ApplicationUser User { get; set; }
    }
}
