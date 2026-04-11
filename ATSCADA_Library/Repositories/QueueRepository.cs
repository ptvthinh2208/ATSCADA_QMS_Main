using ATSCADA_Library.Data;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.Pagination;
using ATSCADA_Library.Helpers.Sorting;
using ATSCADA_Library.Interfaces.Server;
using ATSCADA_Library.Services.PrintingService;
using DocumentFormat.OpenXml.InkML;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace ATSCADA_Library.Repositories
{
    public class QueueRepository : IQueueRepository
    {
        private readonly ATSCADADbContext _context;
        private readonly ZaloApiService _zaloApiService;
        private readonly TicketPrinterService _printerService;

        public QueueRepository(ATSCADADbContext context, ZaloApiService zaloApiService, TicketPrinterService printerService)
        {
            _context = context;
            _zaloApiService = zaloApiService;
            _printerService = printerService;
        }

        public async Task<Queue> GetQueueByOrderNumberAndCounterId(int orderNumber, int counterId)
        {
            if (orderNumber != 0)
            {
                var today = DateTime.Today.Date;
                var result = await _context.Queues.Where(x => x.OrderNumber == orderNumber 
                && x.CounterId == counterId
                && x.AppointmentDate.Date == today).FirstOrDefaultAsync();
                return result!;
            }
            else return null!;
        }
        //Lấy thông tin khách hàng nếu khách hàng chuyển từ quầy khác tới
        public async Task<Queue> GetQueueByOrderNumberAndCounterId(int orderNumber, int counterId, int previousCounterId)
        {
            if (orderNumber != 0 && previousCounterId != 0)
            {
                var today = DateTime.Today.Date;
                var result = await _context.Queues.Where(x => x.OrderNumber == orderNumber
                && x.CounterId == counterId && x.OriginalCounterId == previousCounterId && x.AppointmentDate.Date == today).FirstOrDefaultAsync();
                return result!;
            }
            else return null!;
        }
        public async Task<List<Queue>> GetAllQueueByServiceId(long id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid ID provided", nameof(id));
            }
            var Service = await _context.Queues
                .Where(x => x.ServiceId == id).OrderBy(x => x.PrintTime)
                .ToListAsync();
            return Service;
        }
        public async Task<Queue> CreateQueueAsync(QueueDto model)
        {
            try
            {
                //int orderNumber = 0;
                //DateTime estimatedTime;
                //var selectedCounter = new Counter();
                //var Service = await _context.Services.FindAsync(model.ServiceId);
                //var today = DateTime.Today.Date;
                //if (Service == null)
                //{
                //    return null!;
                //}
                //else
                //{
                //    //Lấy danh sách Counter thuộc ServiceID đã chọn
                //    var counters = _context.Counters.Where(x => x.ServiceID == model.ServiceId && x.IsActive == true).ToList();
                //    //Nếu khách hàng đặt lịch trong ngày
                //    if (model.AppointmentDate.Date == today)
                //    {
                //        // Lấy Counter có số lượng phiếu đã in ít nhất, nếu bằng nhau thì lấy CurrentNumber lớn nhất
                //        selectedCounter = counters
                //            .OrderBy(c => c.TotalCount)             // Ưu tiên TotalCount ít nhất
                //            .ThenByDescending(c => c.CurrentNumber) // Nếu TotalCount giống nhau, ưu tiên CurrentNumber lớn nhất
                //            .ThenBy(c => c.CreatedDate)               // Sau cùng nếu cả TotalCount và CurrentNumber đều giống nhau thì ưu tiên hàng chờ nào được tạo trước
                //            .FirstOrDefault();

                //        if (selectedCounter != null)
                //        {
                //            orderNumber = selectedCounter!.TotalCount + 1;
                //            var numberOfWaitingClients = orderNumber - selectedCounter.CurrentNumber;
                //            var averageServiceTime = selectedCounter.AverageServiceTime; // thời gian phục vụ trung bình của quầy
                //            estimatedTime = DateTime.Now + (averageServiceTime * numberOfWaitingClients);
                //        }
                //        else
                //        {
                //            throw new Exception("No counter available for today.");
                //        }

                //    }
                //    //Nếu khách hàng đặt lịch ở tương lai
                //    else
                //    {
                //        // Phân phối khách hàng vào quầy dựa trên số lượng khách hàng hiện tại
                //        selectedCounter = counters
                //            .Select(counter => new
                //            {
                //                Counter = counter,
                //                TotalCount = _context.Queues
                //                    .Where(q => q.CounterId == counter.Id && q.AppointmentDate.Date == model.AppointmentDate.Date)
                //                    .Count()
                //            })
                //            .OrderBy(x => x.TotalCount) // Ưu tiên quầy có ít khách nhất
                //            .ThenBy(x => x.Counter.CreatedDate) // Nếu bằng nhau, chọn quầy được tạo trước
                //            .FirstOrDefault()?.Counter;

                //        if (selectedCounter != null)
                //        {
                //            // Xác định OrderNumber cho khách hàng tại quầy đã chọn
                //            orderNumber = _context.Queues
                //                .Where(q => q.CounterId == selectedCounter.Id && q.AppointmentDate.Date == model.AppointmentDate.Date)
                //                .Count() + 1;
                //        }
                //        // Đặt thời gian dự kiến mặc định luôn là 8h sáng
                //        var appointmentDate = model.AppointmentDate.Date;
                //        estimatedTime = appointmentDate.AddHours(8);
                //    }

                //}
                var setting = _context.Settings.FirstOrDefault(); //Lấy thông tin cài đặt
                var Service = await _context.Services.FindAsync(model.ServiceId);
                if (Service == null)
                {
                    Console.WriteLine("Service not found.");
                    return new Queue();
                }
                //Lấy danh sách Counter thuộc ServiceID đã chọn
                //var counters = _context.Counters.Where(x => x.ServiceID == model.ServiceId).ToList();
                var assignedCounter = await GetAssignedCounter(Service);
                //var today = DateTime.Today.Date;


                // Create a new Queue instance
                var newQueue = new Queue
                {
                    OrderNumber = Service.TotalTicketPrint + 1,
                    //AppointmentId = model.Id,
                    ServiceId = model.ServiceId,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    IdentificationNumber = model.IdentificationNumber,
                    AppointmentDate = model.AppointmentDate,
                    //AppointmentTime = estimatedTime,
                    Message = model.Message,
                    Status = "Waiting",
                    Verified = true,
                    DescriptionService = assignedCounter.Code, //Lấy code của counter làm tiền tố
                    NameService = Service.Name,
                    //NameCounter = selectedCounter!.Name,
                    CounterId = 0,
                    OriginalCounterId = 0,
                    PrintTime = DateTime.Now,
                    Priority = false
                };
                //Send ZNS Zalo
                //var setting = _context.Settings.FirstOrDefault();
                //if (setting!.IsActiveSendZNS && setting.SendZnsWithTemplate != null)
                //{
                //    await _zaloApiService.SendTemplateMessageAsync(newQueue, selectedCounter, setting.SendZnsWithTemplate);
                //}

                // Add the new Queue to the context
                _context.Queues.Add(newQueue);

               

                if (model.isPrinting)
                {
                    var printRequest = new PrintRequest()
                    {
                        FullName = newQueue.FullName!,
                        ServiceName = Service.Description!,
                        TicketNumber = assignedCounter.Code + newQueue.OrderNumber.ToString("D3"),
                        AppointmentTime = newQueue.AppointmentTime.ToString("dd/MM/yyyy hh:mm"),
                        PrinterIP = model.PrinterIP!,
                        //VehicleNumber = newQueue.VehicleNumber!
                    };
                    _printerService.PrintTicket(printRequest);
                }

                // Save the changes to the database
                await _context.SaveChangesAsync();


                return newQueue;
            }
            catch(Exception ex)
            {
                return null;
            }
        }
        public async Task<Queue> GetQueueById(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid ID provided", nameof(id));
            }

            var queue = await _context.Queues.FindAsync(id);

            if (queue == null)
            {
                throw new KeyNotFoundException($"Service with ID {id} not found.");
            }

            return queue;
        }
        public async Task<List<Queue>> GetAllQueueAsync()
        {
            var today = DateTime.Today;
            return await _context.Queues
                .Include(q => q.Service)
                .Where(x => x.AppointmentDate.Date == today.Date).ToListAsync();
        }
        public async Task<List<Queue>> GetQueuesByCounterIdAsync(int counterId)
        {
            var today = DateTime.Today;
            return await _context.Queues
                             .Where(q => q.CounterId == counterId && q.AppointmentDate.Date == today.Date)
                             .OrderByDescending(q => q.Priority)
                             .ToListAsync();
        }
        //public async Task<Queue> GetQueueByOrderNumberAndCounterNameAsync(int orderNumber, string counterName)
        //{
        //    if (orderNumber != 0)
        //    {
        //        var result = await _context.Queues.Where(x => x.OrderNumber == orderNumber && x.NameCounter == counterName).FirstOrDefaultAsync();
        //        return result!;
        //    }
        //    else return null!;
        //}
        public async Task<List<Queue>> GetQueuesByStatusesAsync(List<string> statuses, long ServiceId)
        {
            if (ServiceId > 0)
            {
                return await _context.Queues
                .Where(q => statuses.Contains(q.Status!) && q.ServiceId == ServiceId).OrderByDescending(q => q.Priority).ThenBy(q => q.PrintTime)
                .ToListAsync();
            }
            return await _context.Queues
               .Where(q => statuses.Contains(q.Status!)).OrderBy(q => q.PrintTime)
               .ToListAsync();
        }
        public async Task<List<Service>> GetServiceList()
        {
            return await _context.Services.ToListAsync();
        }
        public async Task<PagedList<Service>> GetServiceList(PagingParameters paging)
        {

            // Start building the query by searching and optionally applying a date range filter
            var query = _context.Services
                                .Search(paging.SearchTerm!, "Name")
                                .AsQueryable();
            // Apply sorting (descending or ascending)
            if (paging.SortOrder == "desc")
            {
                var orderByDesc = string.Concat(paging.SortBy, " desc");
                query = query.Sort(orderByDesc);  // Assuming Sort extension method supports sorting by dynamic fields
            }
            else
            {
                query = query.Sort(paging.SortBy);  // Default sort order is ascending
            }

            // If PageSize is 0 or not provided, return all items without paging
            if (paging.PageSize == 0)
            {
                // Return all records without paging
                var fullList = await query.ToListAsync();
                return new PagedList<Service>(fullList, fullList.Count, 1, fullList.Count);
            }

            // Execute the query and paginate the results
            var list = await query.ToListAsync();
            return PagedList<Service>.ToPagedList(list, paging.PageNumber, paging.PageSize);
        }
        private async Task<Counter?> GetAssignedCounter(Service Service)
        {
            return await _context.Counters.FirstOrDefaultAsync(x => x.ServiceID == Service.Id);
        }
        public async Task<Service> GetServiceById(long id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid ID provided", nameof(id));
            }

            var Service = await _context.Services.FindAsync(id);

            if (Service == null)
            {
                throw new KeyNotFoundException($"Service with ID {id} not found.");
            }

            return Service;
        }
        public async Task<Queue> UpdateQueueAsync(Queue model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Queues.Update(model);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return model;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
            //    _context.Queues.Update(model);
            //await _context.SaveChangesAsync();
            //return model;
        }
        public async Task<Queue> UpdateTransferQueueAsync(Queue oldModel, long newServiceId, int counterId)
        {
            //int orderNumber = 0;
            var selectedCounter = new Counter();
            var newService = await _context.Services.FindAsync(newServiceId);
            if (newService == null)
            {
                throw new InvalidOperationException("Service not found.");
            }
            else
            {
                //Lấy danh sách Counter thuộc ServiceID đã chọn
                var counters = _context.Counters.Where(x => x.ServiceID == newServiceId).ToList();
                // Lấy counter đã chọn cho User chuyển quầy
                selectedCounter = counters.Where(x => x.Id == counterId).FirstOrDefault();

                ////Lấy dữ liệu cuối cùng theo ServiceId
                //var lastRecord = await _context.Queues
                //    .Where(x => x.ServiceId == newService.Id)
                //    .OrderByDescending(x => x.Id)
                //    .FirstOrDefaultAsync();
                ////Nếu hàng chờ chưa có dữ liệu, OrderNumber mặc định = 1
                //if (lastRecord == null)
                //{
                //    orderNumber = 1;
                //}
                //else
                //{
                //    orderNumber = lastRecord!.OrderNumber + 1;
                //}
            }
            //Them record với dữ liệu mới
            var newModel = new Queue
            {
                OrderNumber = oldModel.OrderNumber,
                AppointmentId = oldModel.Id,
                ServiceId = newServiceId,
                FullName = oldModel.FullName,
                PhoneNumber = oldModel.PhoneNumber,
                AppointmentDate = oldModel.AppointmentDate,
                AppointmentTime = oldModel.AppointmentTime,
                Message = oldModel.Message,
                Status = "Waiting",
                Verified = true,
                DescriptionService = newService.Description,
                NameService = newService.Name,
                CounterId = counterId,
                OriginalCounterId = oldModel.CounterId,
                PrintTime = DateTime.Now,
                Priority = false
            };

            _context.Queues.Add(newModel); //Thêm mới 1 dòng dữ liệu với dữ liệu được thay đổi từ hàng chờ khác
            await _context.SaveChangesAsync();
            return newModel;
        }
        public async Task<List<Queue>> UpdateTransferQueueAsync(List<Queue> oldModels, long newServiceId, int counterId)
        {
            var updatedQueues = new List<Queue>(); // Danh sách lưu trữ kết quả đã cập nhật

            // Lấy Service mới dựa trên ID
            var newService = await _context.Services.FindAsync(newServiceId);
            if (newService == null)
            {
                throw new InvalidOperationException("Service not found.");
            }

            // Lấy danh sách Counter thuộc ServiceID đã chọn
            var counters = _context.Counters.Where(x => x.ServiceID == newServiceId).ToList();

            foreach (var oldModel in oldModels)
            {
                // Lấy counter đã chọn cho User chuyển quầy
                var selectedCounter = counters.FirstOrDefault(x => x.Id == counterId);
                if (selectedCounter == null)
                {
                    throw new InvalidOperationException($"Counter with ID {counterId} not found.");
                }

                // Tạo đối tượng Queue mới từ dữ liệu cũ và thông tin mới
                var newModel = new Queue
                {
                    OrderNumber = oldModel.OrderNumber,
                    AppointmentId = oldModel.Id,
                    ServiceId = newServiceId,
                    FullName = oldModel.FullName,
                    PhoneNumber = oldModel.PhoneNumber,
                    AppointmentDate = oldModel.AppointmentDate,
                    AppointmentTime = oldModel.AppointmentTime,
                    Message = oldModel.Message,
                    Status = "Waiting", // Cập nhật status khi chuyển quầy
                    Verified = true,
                    DescriptionService = newService.Description,
                    NameService = newService.Name,
                    CounterId = counterId,  // Quầy mới được chuyển tới
                    OriginalCounterId = oldModel.CounterId, // Quầy gốc của khách hàng
                    PrintTime = DateTime.Now,
                    Priority = false
                };

                _context.Queues.Add(newModel);  // Thêm mới dữ liệu vào cơ sở dữ liệu
                updatedQueues.Add(newModel);  // Thêm vào danh sách kết quả
            }

            // Lưu tất cả các thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            return updatedQueues; // Trả về danh sách các Queue đã được cập nhật
        }

        public async Task<Service> UpdateService(Service model)
        {
            _context.Services.Update(model);
            await _context.SaveChangesAsync();
            return model;
        }
        public async Task<Service> CreateServiceAsync(Service model)
        {
            var existingService = await _context.Services
                                             .FirstOrDefaultAsync(u => u.Name == model.Name);
            if (existingService != null)
            {
                throw new Exception("A Service with this name already exists.");
            }
            var newService = new Service
            {
                Name = model.Name,
                IsActive = true,
                Description = model.Description,
                CreatedBy = model.CreatedBy,
                CreatedDate = DateTime.Now,

            };
            await _context.Services.AddAsync(newService);
            await _context.SaveChangesAsync();
            return newService;
        }

        public async Task<Service> CheckNameService(ServiceDto dto)
        {

            var result = await _context.Services.Where(x => x.Name == dto.Name).FirstOrDefaultAsync();
            return result!;

        }
        //Cập nhật số lượng phiếu đã in khi có 1 khách hàng đăng ký dịch vụ thành công.
        public async Task UpdateTotalTicketPrintDirectly(long ServiceId)
        {
            await _context.Services
                .Where(q => q.Id == ServiceId)
                .ExecuteUpdateAsync(q => q.SetProperty(qi => qi.TotalTicketPrint, qi => qi.TotalTicketPrint + 1));
        }

    }
}
