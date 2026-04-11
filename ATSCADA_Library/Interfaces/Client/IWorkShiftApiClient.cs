using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Interfaces.Client
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
