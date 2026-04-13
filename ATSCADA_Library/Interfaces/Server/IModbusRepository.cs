using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;

namespace ATSCADA_API.Interfaces
{
    public interface IModbusRepository
    {
        Task<Modbus?> GetByIdAsync(int id);
        Task<IEnumerable<Modbus>> GetAllAsync();
        Task AddAsync(Modbus modbus);
        Task UpdateAsync(Modbus modbus);
        Task DeleteAsync(int id);
    }
}
