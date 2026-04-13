using ATSCADA_API.Hubs;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using ATSCADA_Library.Interfaces.Server;
using ATSCADA_Library.Repositories;
using ATSCADA_Library.Services.ApiService;
using ATSCADA_Library.Services.ApiService.Interfaces;
using ATSCADA_Library.Services.BackgroundServices;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ATSCADA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueueController : ControllerBase
    {
        private readonly IQueueRepository _queueRepository;
        private readonly CounterService _counterService;
        private readonly IHubContext<QueueHub> _hubContext;
        private readonly IModbusService _modbusService;

        public QueueController(
            IQueueRepository queueRepository, 
            CounterService counterService, 
            IHubContext<QueueHub> hubContext,
            IModbusService modbusService)
        {
            _queueRepository = queueRepository;
            _counterService = counterService;
            _hubContext = hubContext;
            _modbusService = modbusService;
        }


        //Get all Queue
        [HttpGet("get-all-queue")]
        public async Task<IActionResult> GetAllQueue()
        {
            var list = await _queueRepository!.GetAllQueueAsync();
            return Ok(list);
        }
        //Lấy danh sách khách hàng theo tên quầy 
        [HttpGet("get-queues-by-counter-id")]
        public async Task<IActionResult> GetQueuesByCounterName(int counterId)
        {
            var list = await _queueRepository.GetQueuesByCounterIdAsync(counterId);
            return Ok(list);
        }
        [HttpGet("get-queues-by-statuses")]
        public async Task<IActionResult> GetQueuesByStatuses([FromQuery] List<string> statuses, [FromQuery] long ServiceId)
        {
            var list = await _queueRepository.GetQueuesByStatusesAsync(statuses, ServiceId);
            return Ok(list);
        }
        //Get all Queue by ServiceID
        [HttpGet("get-all-queue-by-ServiceID/{ServiceId}")]
        public async Task<IActionResult> GetAllQueue(long ServiceId)
        {
            var list = await _queueRepository!.GetAllQueueByServiceId(ServiceId);
            return Ok(list);
        }
        //Lấy thông tin khách hàng theo CounterId và số thứ tự
        [HttpGet("get-queue-by-ordernumber-and-counterId")]
        public async Task<IActionResult> GetQueueByOrderNumberAndCounterId(int orderNumber, int counterId, int previousCounterId)
        {
            if (previousCounterId == 0)
            {
                var result = await _queueRepository!.GetQueueByOrderNumberAndCounterId(orderNumber, counterId);
                if (result != null)
                {
                    return Ok(result);
                }
            }
            else
            {
                var result = await _queueRepository!.GetQueueByOrderNumberAndCounterId(orderNumber, counterId, previousCounterId);
                if (result != null)
                {
                    return Ok(result);
                }
            }
            return NotFound(); // Trả về 404 nếu không tìm thấy
        }
        ////Lấy thông tin khách hàng theo số thứ tự và tên quầy
        //[HttpGet("get-queue-by-ordernumber-and-counterName")]
        //public async Task<IActionResult> GetQueueByOrderNumberAndCounterName(int orderNumber, string counterName)
        //{
        //    var result = await _queueRepository!.GetQueueByOrderNumberAndCounterNameAsync(orderNumber, counterName);
        //    if (result != null)
        //    {
        //        return Ok(result);
        //    }
        //    else return Ok(new Queue());
        //}


        // CreateQueue method using QueueService
        [HttpPost]
        public async Task<IActionResult> CreateQueue([FromBody] QueueDto dto)
        {
            try
            {
                var today = DateTime.Today;
                if (ModelState.IsValid)
                {
                    var res = await _queueRepository.CreateQueueAsync(dto);
                    //Kiểm tra nếu đặt lịch trong ngày hôm nay thì mới update
                    if (res != null && res.AppointmentDate.Date == today.Date)
                    {
                        //Cập nhật giá trị TotalTiketPrint lên 1 
                        await _queueRepository.UpdateTotalTicketPrintDirectly(dto.ServiceId);
                        // Notify all clients about the queue creation
                        await _hubContext.Clients.All.SendAsync("ReceiveMessage");
                        return Ok(res);
                    };
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //Update 
        [HttpPut("update-queue/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] QueueUpdateRequest request)
        {
            if (id != 0)
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var resultFromDb = await _queueRepository.GetQueueById(id);

                if (resultFromDb == null)
                {
                    return NotFound($"{id} is not found");
                }
                if (resultFromDb.Priority != request.Priority)
                {
                    resultFromDb.Priority = request.Priority;
                }
                if (resultFromDb.Status != request.Status)
                {
                    resultFromDb.Status = request.Status;
                    resultFromDb.LastTimeUpdated = DateTime.Now;
                }
                if (resultFromDb.CounterId != request.CounterId)
                {
                    resultFromDb.CounterId = request.CounterId;
                }
                if (resultFromDb.OriginalCounterId != request.OriginalCounterId)
                {
                    resultFromDb.OriginalCounterId = request.OriginalCounterId;
                }
                if (resultFromDb.isNotified != request.isNotified)
                {
                    resultFromDb.isNotified = request.isNotified;
                }
                var Result = await _queueRepository.UpdateQueueAsync(resultFromDb);

                // Notify all clients about the queue updated
                await _hubContext.Clients.All.SendAsync("ReceiveMessage");
                return Ok(new Queue()
                {
                    Id = Result.Id,
                    Priority = Result.Priority,
                    Status = Result.Status,
                });
            }
            else return NoContent();
        }
        [HttpPost("counter/{counterId}/call-next")]
        public async Task<IActionResult> CallNextTicket(int counterId, [FromBody] CallNextRequest request)
        {
            var result = await _counterService.CallNextTicketAtomicAsync(counterId, request.ServiceId);

            if (result.Queue == null || result.Counter == null)
                return Ok(new { Success = false, Message = "Không còn khách chờ" });

            // Tính toán giá trị LED từ Code và OrderNumber
            // var ledValue = ushort.Parse(result.Counter.Code + result.Queue.OrderNumber.ToString("D3"));

            // Gửi tín hiệu ngay lập tức qua Channel thay vì chờ poll
            // await _ledSyncService.TriggerLedUpdateAsync(result.Counter.ModbusId, ledValue);

            // Notify SignalR clients về việc có queue mới
            await _hubContext.Clients.All.SendAsync("ReceiveMessage");

            return Ok(new
            {
                Success = true,
                Queue = result.Queue,
                Counter = result.Counter,
                CurrentNumber = result.Queue.OrderNumber
                // LedValue = ledValue
            });
        }

        [HttpPost("counter/{counterId}/call-previous")]
        public async Task<IActionResult> CallPreviousTicket(int counterId, [FromBody] CallNextRequest request)
        {
            var result = await _counterService.CallPreviousTicketAtomicAsync(counterId, (int)request.ServiceId);

            if (result.Queue == null || result.Counter == null)
                return Ok(new { Success = false, Message = "Không còn khách phía trước" });

            // Notify SignalR clients về việc có queue mới
            await _hubContext.Clients.All.SendAsync("ReceiveMessage");

            return Ok(new
            {
                Success = true,
                Queue = result.Queue,
                Counter = result.Counter,
                CurrentNumber = result.Queue.OrderNumber
            });
        }
        //Update 
        [HttpPut("update-transfer-queue/list/{newServiceID}")]
        public async Task<IActionResult> Update(long newServiceID, int counterId, [FromBody] List<Queue> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var Result = await _queueRepository.UpdateTransferQueueAsync(request, newServiceID, counterId);
            foreach (var item in Result)
            {
                //Update total count
                await _counterService.UpdateTotalCountAsync(item.CounterId!);
                await Task.Delay(100);
            }
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveMessage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to SignalR clients: {ex.Message}");
            }
            return Ok(Result);
        }
        //Update 
        [HttpPut("update-transfer-queue/single/{newServiceID}")]
        public async Task<IActionResult> UpdateTransferSingle(long newServiceID, int counterId, [FromBody] Queue request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var Result = await _queueRepository.UpdateTransferQueueAsync(request, newServiceID, counterId);

            await _counterService.UpdateTotalCountAsync(counterId);
            await Task.Delay(100);

            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveMessage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to SignalR clients: {ex.Message}");
            }
            return Ok(Result);
        }
    }
}
