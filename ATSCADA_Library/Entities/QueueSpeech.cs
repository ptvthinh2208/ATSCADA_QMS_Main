using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Entities
{
    public class QueueSpeech
    {
        public Guid Id { get; set; }
        public string? TextToSpeech {  get; set; }
        
        
        public DateTime CreatedDate { get; set; }
        public DateTime SpeechAt {  get; set; }
        public bool IsCompleted { get; set; }
    }
}
