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
    public class SystemRoleRepository : ISystemRoleRepository
    {
        private readonly ATSCADADbContext _context;
        public SystemRoleRepository(ATSCADADbContext context)
        {
            _context = context;
        }
        public async Task<List<SystemRole>> GetAllRoleAsync()
        {
            return await _context.SystemRoles.ToListAsync();
        }
    }
}
