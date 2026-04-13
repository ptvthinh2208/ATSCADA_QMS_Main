using ATSCADA_Library.DTOs;
using ATSCADA_Library.Interfaces.Server;
using Microsoft.AspNetCore.Mvc;

namespace ATSCADA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        //private readonly IAppointmentRepository? _appointmentRepository;

        //public AppointmentController(IAppointmentRepository appointmentRepository)
        //{
        //    _appointmentRepository = appointmentRepository;
        //}
        //[HttpGet("get-all-appointment")]
        //public async Task<IActionResult> Get()
        //{
        //    var result = await _appointmentRepository!.GetAppointmentsAsync();

        //    //Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(products.MetaData));

        //    return Ok(result);
        //}
        ////api/appointment?phoneNumber = 
        //[HttpGet("get-appointment-by-phoneNumber/{phoneNumber}")]
        //public async Task<IActionResult> GetLastAppointment(string phoneNumber)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);
        //    var result = await _appointmentRepository!.GetLastAppointmentAsync(phoneNumber);
        //    if (result.Verified == false)
        //    {
        //        if (result.Status == "Unverified")
        //        {
        //            return Ok(result);
        //        }
        //        else
        //        {
        //            return BadRequest();
        //        }
        //    }
        //    return BadRequest();
        //}
        //[HttpPost]
        //public async Task<IActionResult> CreateAppointment([FromBody] AppointmentDto dto)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            var res = await _appointmentRepository!.CreateAppointmentAsync(dto);
        //            return Ok(res);
        //        }
        //        return BadRequest();
        //    }
        //    catch (Exception ex)
        //    {

        //        return BadRequest(ex.Message);
        //    }
        //}
        //[HttpPatch("update-status-appointment-verified/{phoneNumber}")]
        //public async Task<IActionResult> UpdateAppointmentVerified(string phoneNumber, AppointmentUpdateStatusRequest request)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);
        //    var resultFromDb = await _appointmentRepository!.GetLastAppointmentAsync(phoneNumber);
        //    if (resultFromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    //Cập nhật trạng thái từ "Unverify" sang "Waiting" và "Verified" thành True
        //    resultFromDb.Status = request.Status;
        //    resultFromDb.Verified = request.Verified;

        //    var appointmentResult = await _appointmentRepository.UpdateAppointmentAsync(resultFromDb);
        //    return Ok(appointmentResult);
        //}

    }
}
