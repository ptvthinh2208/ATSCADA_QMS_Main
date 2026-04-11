using ATSCADA_Library.Helpers.Pagination;
using ATSCADA_Library.Interfaces.Server;
using ATSCADA_Library.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ATSCADA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkShiftController : ControllerBase
    {
        private readonly IWorkShiftRepository _workShiftRepository;
        public WorkShiftController(IWorkShiftRepository workShiftRepository)
        {
            _workShiftRepository = workShiftRepository;
        }
        [HttpGet("get-all-workshift")]
        public async Task<IActionResult> GetAll()
        {
            var workShifts = await _workShiftRepository.GetListWorkShiftAsync();
            return Ok(workShifts);
        }
    }
}
