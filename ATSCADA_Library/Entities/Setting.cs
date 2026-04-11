using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;


namespace ATSCADA_Library.Entities
{
    [Table("Settings")]
    public class Setting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public bool IsAppointmentForm { get; set; }
        public bool IsActiveSendZNS { get; set; }
        public string? SendZnsWithTemplate {  get; set; }
        public int EstimatedCompletionTime { get; set; } = 10; // Thời gian dự kiến hoàn thành 10phut
        public bool IsActiveSpeechCall { get; set; }
        public TimeSpan? ScheduledTaskTime { get; set; }
        public DateTime? LastTaskExecuted { get; set; }
        //FooterSetting
        public string? FooterTextCountersMainView {  get; set; }
        public string? FooterTextColor { get; set; } // Màu chữ
        public int FooterTextFontSize { get; set; } = 24; // Cỡ chữ 
        public string? UrlVideoCountersMainView { get; set; }
        public int MaxVisibleCounters { get; set; }
    }
}
