using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Interfaces.Server;
using ATSCADA_Library.Services.BackgroundServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace ATSCADA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingController : ControllerBase
    {
        private readonly ISettingRepository? _settingRepository;
        private readonly QueueResetService _queueResetService;
        private readonly AppointmentService _appointmentService;
        private readonly CounterServiceBackground _counterService;
        private readonly DailyReportService _dailyReportService;

        public SettingController(ISettingRepository settingRepository,
            QueueResetService queueResetService,
            AppointmentService appointmentService,
            CounterServiceBackground counterService,
            DailyReportService dailyReportService)
        {
            _settingRepository = settingRepository;
            _queueResetService = queueResetService;
            _appointmentService = appointmentService;
            _counterService = counterService;
            _dailyReportService = dailyReportService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllSettingsAsync()
        {
            var result = await _settingRepository!.GetAllSettingsAsync();
            return Ok(result);
        }
        //[HttpPatch("IsAppointmentForm")]
        //public async Task<IActionResult> UpdateAppointment(Setting isAppointment)
        //{
        //    var resultData = isAppointment.IsAppointmentForm;
        //    var result = await _settingRepository!.UpdateIsAppoitmentAsync(resultData);
        //    return Ok(result);
        //}
        //[HttpPatch("ZNS-Time-Notification")]
        //public async Task<IActionResult> UpdateZNSTimeNotification(Setting znsTimeNotification)
        //{
        //    var resultData = 0; //znsTimeNotification.ZNSTimeNotification;
        //    var result = await _settingRepository!.UpdateZNSTimeNotificationAsync(resultData);
        //    return Ok(result);
        //}
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateSetting(long id, [FromBody] SettingUpdateRequest updateRequest)
        {
            try
            {
                await _settingRepository!.UpdateSettingAsync(id, updateRequest);
                return NoContent(); // 204 No Content on success
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("run-now")]
        public async Task<IActionResult> RunNow(CancellationToken cancellationToken)
        {
            try
            {
                // Reset hàng chờ
                await _queueResetService.ResetQueuesAsync(cancellationToken);

                // Lấy lịch hẹn cho ngày hiện tại
                var appointments = await _appointmentService.GetAppointmentsForTodayAsync(cancellationToken);

                // Cập nhật tổng số khách đặt lịch
                 await _counterService.UpdateTotalCountForDateAsync(appointments, cancellationToken);

                // Tạo báo cáo hàng ngày
                await _dailyReportService.GenerateDailyReportAsync(cancellationToken);

                return Ok(new { message = "Hàng chờ đã được reset thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi reset hàng chờ.", error = ex.Message });
            }
        }
        
    }
}
