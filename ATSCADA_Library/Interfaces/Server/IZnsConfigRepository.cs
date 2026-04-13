using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Interfaces.Server
{
    public interface IZnsConfigRepository
    {
        Task<ZnsConfig> GetZnsConfigAsync();
        Task<ZnsTemplate> GetZnsTemplateAsync();
        Task<List<ZnsInfoDto>> GetZnsInfoAsync();
        Task UpdateZnsConfigAsync(ZnsInfoDto znsInfoDto);
    }
}
