using ATSCADA_Library.DTOs;

namespace ATSCADA_Client.Interfaces.Client
{
    public interface IZnsConfigApiClient
    {
        Task<List<ZnsInfoDto>> GetZnsInfoAsync();
        Task UpdateZnsInfoAsync(ZnsInfoDto znsInfoDto);
    }
}
