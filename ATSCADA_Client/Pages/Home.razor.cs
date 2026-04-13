using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace ATSCADA_Client.Pages
{
    public partial class Home
    {
        private HubConnection? hubConnection;
        private List<QueueHistory> listQueueHistoriesOfThisWeek = new List<QueueHistory>();
        private List<QueueHistory> listQueueHistoriesOfThisMonth = new List<QueueHistory>();
        private List<QueueHistory> listQueueHistoriesOfThisYear = new List<QueueHistory>();
        private PagingParameters paging = new PagingParameters();
        private List<Queue> ListAllQueue = new List<Queue>();
        private List<Counter> ListAllCounter = new List<Counter>();
        private List<Service> ListAllService = new List<Service>();
        private List<ApplicationUser>? ListUsers = new List<ApplicationUser>();
        private Setting dataSetting = new Setting();
        private string? DatetimeToday { get; set; } = DateTime.Now.ToString("dd/MM/yyyy");
        private string? DatetimeTomonth { get; set; } = DateTime.Now.ToString("MM/yyyy");
        private DateTime StartDateOfMonth { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);  // Ngày 1 của tháng hiện tại
        private DateTime EndDateOfMonth { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)).AddDays(1).AddSeconds(-1);  // Ngày cuối cùng của tháng hiện tại
        private DateTime StartDateOfWeek { get; set; }
        private DateTime EndDateOfWeek { get; set; }
        private DateTime StartDateOfYear { get; set; } = new DateTime(DateTime.Now.Year, 1, 1);
        private DateTime EndDateOfYear { get; set; } = new DateTime(DateTime.Now.Year, 12, 31);
        //private 
        private int TotalBookingOfThisWeek { get; set; }

        private int TotalBookingOfThisMonth { get; set; }
        private int TotalBookingOfToday { get; set; }
        private int TotalBookingOfThisYear { get; set; }
        private int MaxVisibleCounters { get; set; }
        private string? ManagerId;
        private bool IsManager;
        private string? UserNameFromJWT { set; get; }
        private bool _isLoading = false;
        private bool isConnectionDisposed = false;
        private async Task DisposeConnection()
        {
            if (hubConnection != null && !isConnectionDisposed)
            {
                isConnectionDisposed = true;
                await hubConnection.StopAsync();
                await hubConnection.DisposeAsync();
                hubConnection = null;
                Console.WriteLine("SignalR connection stopped and disposed.");
                isConnectionDisposed = false;
            }
        }
        protected override async Task OnInitializedAsync()
        {
            try
            {
                await GetAllQueueHistoriesOnWeek();
                await GetAllQueueHistoriesOnMonth();
                await GetAllQueueHistoriesOnYear();
                await GetListAllQueue();
                await GetListAllService();
                TotalBookingOfToday = ListAllQueue.Count();
                dataSetting = await SettingApiClient.GetAllSettings();
                MaxVisibleCounters = dataSetting.MaxVisibleCounters;


                // Đợi SignalR kết nối xong rồi mới tiếp tục
                await EnsureSignalRConnected();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting SignalR connection: {ex.Message}");
            }
        }
        // Phương thức đảm bảo SignalR đã kết nối
        private async Task EnsureSignalRConnected()
        {
            if (HubConnection.State == HubConnectionState.Disconnected)
            {
                await HubConnection.StartAsync();
            }

            // Gọi lại dữ liệu sau khi đảm bảo SignalR đã kết nối
            CallLoadData();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            try
            {
                if (firstRender)
                {
                    //Kết nối SignalR để lấy giá trị real time trong database
                    if (HubConnection.State == HubConnectionState.Disconnected)
                    {

                        HubConnection.On("ReceiveMessage", () =>
                        {
                            CallLoadData();
                        });
                        await HubConnection.StartAsync();
                        //Console.WriteLine("SignalR connection started.");

                        // Gọi CallLoadData() ngay sau khi kết nối thành công
                        CallLoadData();

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting SignalR connection: {ex.Message}");
            }
        }
        private void CallLoadData()
        {
            Task.Run(async () =>
            {
                Console.WriteLine("Start loading data...");
                await GetAllQueueHistoriesOnYear();
                await GetAllQueueHistoriesOnWeek();
                await GetAllQueueHistoriesOnMonth();
                await GetListAllQueue();
                await GetListAllService();
                Console.WriteLine("Data loading completed.");
            });
        }
        
        private async Task GetAllQueueHistoriesOnYear()
        {
            var listPagingResponse = await QueueHistoryApiClient.GetAllQueueHistories(paging, StartDateOfYear, EndDateOfYear);
            TotalBookingOfThisYear = listPagingResponse.Items.Count;
            listQueueHistoriesOfThisYear = listPagingResponse.Items;
        }
        private async Task GetAllQueueHistoriesOnMonth()
        {
            var listPagingResponse = await QueueHistoryApiClient.GetAllQueueHistories(paging, StartDateOfMonth, EndDateOfMonth);
            TotalBookingOfThisMonth = listPagingResponse.Items.Count;
            listQueueHistoriesOfThisMonth = listPagingResponse.Items;

        }
        private async Task GetAllQueueHistoriesOnWeek()
        {
            SetStartAndEndOfWeekDates();
            var listPagingResponse = await QueueHistoryApiClient.GetAllQueueHistories(paging, StartDateOfWeek, EndDateOfWeek);
            TotalBookingOfThisWeek = listPagingResponse.Items.Count;
            listQueueHistoriesOfThisWeek = listPagingResponse.Items;
        }
        public async Task GetListAllQueue()
        {
            var listClientToday = await QueueApiClient.GetListAllQueue();
            ListAllQueue = listClientToday.Where(x => x.Status != "Transferred").ToList();

        }
        public async Task GetListAllCounter()
        {
            var listPaging = await CounterApiClient.GetAllCounter(paging);
            ListAllCounter = listPaging.Items
                .OrderByDescending(counter => counter.TotalCount) // Sắp xếp giảm dần theo TotalCount
                .ToList();
        }
        public async Task GetListAllService()
        {
            var listPaging = await QueueApiClient.GetServiceList();
            ListAllService = listPaging
                .OrderByDescending(c => c.TotalTicketPrint) // Sắp xếp giảm dần theo TotalCount
                .ToList();
        }
        public void SetStartAndEndOfWeekDates()
        {
            DateTime now = DateTime.Now;

            // Tính ngày bắt đầu tuần (thứ 2)
            int diffToMonday = (now.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)now.DayOfWeek) - (int)DayOfWeek.Monday;
            StartDateOfWeek = now.AddDays(-diffToMonday).Date;

            // Tính ngày kết thúc tuần (chủ nhật)
            int diffToSunday = (int)DayOfWeek.Sunday - (now.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)now.DayOfWeek);
            EndDateOfWeek = now.AddDays(6 - diffToMonday).Date;

        }
        private async Task ExportToExcel(List<QueueHistory> dataList, DateTime dateTime)
        {
            if (dataList != null && dataList.Any())
            {

                var queueHistories = dataList.ToList();

                // Lấy danh sách CounterId để giảm số lần gọi API
                var counterIds = queueHistories.Select(q => q.CounterId).Distinct();


                var counters = await CounterApiClient.GetCountersByIds(counterIds);

                // Tạo dictionary để tra cứu Counter nhanh hơn
                var counterLookup = counters.ToDictionary(c => c.Id, c => c);

                // Chuyển đổi dữ liệu từ QueueHistory sang QueueHistoryToExportData
                var listDataExport = queueHistories.Select(qh =>
                {
                    var counter = counterLookup.ContainsKey(qh.CounterId) ? counterLookup[qh.CounterId] : null;

                    return new QueueHistoryToExportData
                    {
                        PrintTime = qh.PrintTime,
                        DescriptionService = qh.DescriptionService,
                        FullName = qh.FullName,
                        PhoneNumber = qh.PhoneNumber,
                        CounterName = counter?.Name ?? "Unknown", // Đảm bảo không bị null
                        OrderNumber = $"{counter?.Code ?? "N/A"}-{qh.OrderNumber.ToString("D3")}",
                        //Status = qh.Status
                        Status = qh.Status == "Processing" ? "Completed" : qh.Status,
                        LastTimeUpdated = qh.LastTimeUpdated
                    };
                }).ToList();
                var datetimeFormat = dateTime.ToString("dd/MM/yyyy");
                var nameFile = $"Report_Total_From_{datetimeFormat}";
                var request = new ExportDataRequest<QueueHistoryToExportData>
                {
                    EntityType = nameFile,
                    Model = listDataExport
                };

                byte[] content = await ExportApiClient.ExportToExcel(request);

                // Ensure content is not null before proceeding
                if (content != null && content.Length > 0)
                {
                    var fileName = $"{nameFile}.xlsx";
                    var fileUrl = await CreateFileUrl(content);
                    DownloadFile(fileUrl, fileName);
                }
                else
                {
                    // Handle empty content case if needed
                    throw new Exception("No data returned");
                }
            }
            else ToastService.ShowError("No data to export!");

        }
        //Export Excel với dữ liệu hôm nay
        private async Task ExportToExcel(List<Queue> queueToday)
        {
            if (queueToday != null && queueToday.Any())
            {
                var listDataExport = new List<QueueHistoryToExportData>();
                // Chuyển đổi dữ liệu từ QueueHistory sang QueueHistoryToExportData
                foreach (var qh in queueToday)
                {
                    var counter = await CounterApiClient.GetCounterById(qh.CounterId); // Gọi API để lấy Counter
                    listDataExport.Add(new QueueHistoryToExportData
                    {
                        PrintTime = qh.PrintTime,
                        DescriptionService = qh.DescriptionService,
                        FullName = qh.FullName,
                        PhoneNumber = qh.PhoneNumber,
                        CounterName = counter?.Name ?? "Unknown", // Đảm bảo không bị null
                        OrderNumber = $"{counter?.Code}-{qh.OrderNumber.ToString("D3")}",
                        Status = qh.Status == "Processing" ? "Completed" : qh.Status,
                        LastTimeUpdated = qh.LastTimeUpdated
                    });
                }


                var nameFile = $"Report_{queueToday[0].PrintTime}";
                var request = new ExportDataRequest<QueueHistoryToExportData>
                {
                    EntityType = nameFile,
                    Model = listDataExport
                };

                byte[] content = await ExportApiClient.ExportToExcel(request);

                // Ensure content is not null before proceeding
                if (content != null && content.Length > 0)
                {
                    var fileName = $"{nameFile}.xlsx";
                    var fileUrl = await CreateFileUrl(content);
                    DownloadFile(fileUrl, fileName);
                }
                else
                {
                    // Handle empty content case if needed
                    throw new Exception("No data returned");
                }
            }
            else ToastService.ShowError("No data to export!");

        }
        private async Task<string> CreateFileUrl(byte[] fileContent)
        {
            // Call the JavaScript function to create blob URL
            return await JSRuntime.InvokeAsync<string>("createBlobUrl", fileContent);
        }

        private void DownloadFile(string fileUrl, string fileName)
        {
            JSRuntime.InvokeVoidAsync("downloadFile", fileUrl, fileName);
        }
        public async Task<string> GetUserNameFromJWT()
        {
            var token = await JSRuntime.InvokeAsync<string>("getLocalStorage", "authToken");

            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var userName = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;
                return userName;
            }

            return "0";
        }
    }
}
