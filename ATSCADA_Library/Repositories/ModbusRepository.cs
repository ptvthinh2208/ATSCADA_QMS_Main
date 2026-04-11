using ATSCADA_API.Interfaces;
using ATSCADA_Library.Data;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATSCADA_API.Repositories
{
    public class ModbusRepository : IModbusRepository
    {
        private readonly ATSCADADbContext _context;

        public ModbusRepository(ATSCADADbContext context)
        {
            _context = context;
        }

        public async Task<Modbus?> GetByIdAsync(int id)
        {
            return await _context.Modbuses.FindAsync(id); // Assuming DbSet<Modbus> ModbusConfigs in DbContext
        }

        public async Task<IEnumerable<Modbus>> GetAllAsync()
        {
            return await _context.Modbuses.ToListAsync();
        }

        public async Task AddAsync(Modbus modbus)
        {
            _context.Modbuses.Add(modbus);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Modbus modbus)
        {
            _context.Modbuses.Update(modbus);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var modbus = await GetByIdAsync(id);
            if (modbus != null)
            {
                _context.Modbuses.Remove(modbus);
                await _context.SaveChangesAsync();
            }
        }
    }
}
