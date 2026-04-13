using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Interfaces.Server
{

    public interface ISettingRepository
    {
        Task<Setting> GetAllSettingsAsync();
        //Task<bool> UpdateIsAppoitmentAsync(bool isAppointment);
        Task UpdateSettingAsync(long id, SettingUpdateRequest updateRequest);
        Task<int> UpdateZNSTimeNotificationAsync(int znsTimeNotification);
        Task RunScheduledTaskNow(CancellationToken cancellationToken);
    }
}
