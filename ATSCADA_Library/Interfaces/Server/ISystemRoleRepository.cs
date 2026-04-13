using ATSCADA_Library.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Interfaces.Server
{
    public interface ISystemRoleRepository
    {
        Task<List<SystemRole>> GetAllRoleAsync();


    }
}
