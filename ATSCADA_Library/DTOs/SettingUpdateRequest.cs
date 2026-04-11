using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.DTOs
{
    public class SettingUpdateRequest
    {
        public bool? IsActiveSendZNS { get; set; } // Nullable để kiểm tra nếu có thay đổi
        public bool? IsAppointmentForm { get; set; }
        public bool? IsSpeechCall { get; set; }
        public TimeSpan? ScheduledTaskTime { get; set; }
        public string? FooterTextCountersMainView { get; set; }
        public string? FooterTextColor { get; set; } // Màu chữ
        public int FooterTextFontSize { get; set; } = 24; // Cỡ chữ 
        public string? VideoUrl {  get; set; }
        public int MaxVisibleCounters { get; set; }

        public string? ZnsTemplateName { get; set; }
    }
}
