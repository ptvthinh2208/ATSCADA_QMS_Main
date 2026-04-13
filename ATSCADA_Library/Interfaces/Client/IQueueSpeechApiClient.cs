using ATSCADA_Library.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Interfaces.Client
{
    public interface IQueueSpeechApiClient
    {
        Task<bool> CreateQueueSpeech(QueueSpeech model);
        Task<List<QueueSpeech>> GetQueueSpeechList();
        Task<bool> UpdateQueueSpeechById(QueueSpeech request);
        Task<string> ConvertStringToBase64(string stringInput);
    }
}
