using ATSCADA_Library.Entities;

namespace ATSCADA_Client.Interfaces.Client
{
    public interface ISystemRoleApiClient
    {
        Task<List<SystemRole>> GetAllSystemRole();
    }
}
