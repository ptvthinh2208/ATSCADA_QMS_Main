using ATSCADA_Library.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Interfaces.Client
{
    public interface IZnsConfigApiClient
    {
        Task<List<ZnsInfoDto>> GetZnsInfoAsync();
        Task UpdateZnsInfoAsync(ZnsInfoDto znsInfoDto);
    }
}
