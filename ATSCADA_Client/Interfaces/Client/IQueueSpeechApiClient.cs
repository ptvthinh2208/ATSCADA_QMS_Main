using ATSCADA_Library.Entities;

namespace ATSCADA_Client.Interfaces.Client
{
    public interface IQueueSpeechApiClient
    {
        Task<bool> CreateQueueSpeech(QueueSpeech model);
        Task<List<QueueSpeech>> GetQueueSpeechList();
        Task<bool> UpdateQueueSpeechById(QueueSpeech request);
        Task<string> ConvertStringToBase64(string stringInput);
    }
}
