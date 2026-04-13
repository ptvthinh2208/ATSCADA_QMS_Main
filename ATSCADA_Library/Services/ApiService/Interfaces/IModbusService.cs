using ATSCADA_Library.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Services.ApiService.Interfaces
{
    public interface IModbusService
    {
        Task<ModbusDto?> GetByIdAsync(int id);
        Task<IEnumerable<ModbusDto>> GetAllAsync();
        Task AddAsync(ModbusDto dto);
        Task UpdateAsync(int id, ModbusDto dto);
        Task DeleteAsync(int id);

        // Gửi trực tiếp giá trị LED
        Task TransmitOrderNumberAsync(int modbusId, ushort newValue);

    }
}
