using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;

namespace ATSCADA_Client.Interfaces.Client
{
    public interface ISettingApiClient
    {
        Task<Setting> GetAllSettings();
        //Task<bool> UpdateIsAppointment(bool isAppointment);
        Task<bool> UpdateZNSTimeNotification(int znsTimeNotification);
        Task<bool> RunScheduledTaskNow();
        Task UpdateSettingAsync(long id, SettingUpdateRequest updateRequest);
    }
}
