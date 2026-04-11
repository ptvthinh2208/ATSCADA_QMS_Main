using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Entities
{
    public class ApplicationUser 
    {
        public long Id {  get; set; }
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string? FullName {  get; set; }
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string? UserName {  get; set; }
        public string? Password { get; set; }
       
        public bool Status { get; set; } = false; // On = True; Off = False;
        //FK to SystemRole
        public long SystemRoleId { get; set; }
        //public long ServiceId { get; set; }
        //FK to Counter
        public int CounterId {  get; set; } 
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public string? LastUpdatedBy { get; set; }
        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;
    }
}
