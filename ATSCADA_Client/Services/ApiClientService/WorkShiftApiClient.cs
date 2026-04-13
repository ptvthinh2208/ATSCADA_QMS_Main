using ATSCADA_Client.Interfaces.Client;
using ATSCADA_Library.Entities;
using System.Net.Http.Json;

namespace ATSCADA_Client.Services.ApiClientService
{
    public class WorkShiftApiClient : IWorkShiftApiClient
    {
        public HttpClient? _httpClient;
        public WorkShiftApiClient(HttpClient? httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<bool> CreateNewWorkShifts(WorkShift model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteWorkShifts(WorkShift model)
        {
            throw new NotImplementedException();
        }
        public async Task<List<WorkShift>> GetAllWorkShifts()
        {
            var result = await _httpClient!.GetFromJsonAsync<List<WorkShift>>("api/workshift/get-all-workshift");
            return result!;
        }

        public Task<WorkShift> GetWorkShiftById(long id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateWorkShifts(WorkShift model)
        {
            throw new NotImplementedException();
        }
    }
}
