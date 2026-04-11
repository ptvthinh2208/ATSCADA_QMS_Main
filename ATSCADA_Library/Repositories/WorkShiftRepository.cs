using ATSCADA_Library.Data;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Interfaces.Server;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Repositories
{
    public class WorkShiftRepository : IWorkShiftRepository
    {
        private readonly ATSCADADbContext _context;
        public WorkShiftRepository(ATSCADADbContext context)
        {
            _context = context;
        }
        public Task<WorkShift> CreateNewWorkShiftAsync(WorkShift workShift)
        {
            throw new NotImplementedException();
        }

        public Task DeleteWorkShiftAsync(WorkShift workShift)
        {
            throw new NotImplementedException();
        }

        public async Task<List<WorkShift>> GetListWorkShiftAsync()
        {
            var result = await _context.WorkShifts.ToListAsync();
            return result!;
        }
    }
}
