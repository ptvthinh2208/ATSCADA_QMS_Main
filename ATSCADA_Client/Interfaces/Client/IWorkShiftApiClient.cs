using ATSCADA_Library.Entities;

namespace ATSCADA_Client.Interfaces.Client
{
    public interface IWorkShiftApiClient
    {
        Task<List<WorkShift>> GetAllWorkShifts();
        Task<WorkShift> GetWorkShiftById(long id);
        Task<bool> CreateNewWorkShifts(WorkShift model);
        Task<bool> UpdateWorkShifts(WorkShift model);
        Task<bool> DeleteWorkShifts(WorkShift model);
    }
}
