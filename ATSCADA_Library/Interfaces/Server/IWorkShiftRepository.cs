using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Interfaces.Server
{
    public interface IWorkShiftRepository
    {
        Task<List<WorkShift>> GetListWorkShiftAsync();
        
        Task<WorkShift> CreateNewWorkShiftAsync(WorkShift workShift);
        Task DeleteWorkShiftAsync(WorkShift workShift);
    }
}
