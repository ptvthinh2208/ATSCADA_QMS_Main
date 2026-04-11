using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Library.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Reflection.Metadata;
using static System.Net.WebRequestMethods;

namespace ATSCADA_Client.Pages.Control
{
    public partial class CountersControl
    {
        [Parameter]
        public int CounterId { set; get; }
        //private string CounterName { set; get; } = "A01";
        //private int CounterId { set; get; } = 1;
        private long ServiceId { set; get; }
        private int selectedCounterId { get; set; }
        private Counter? selectedCounter; // Lưu đối tượng Counter đã chọn
        private int selectedCounterName { get; set; }
        private int previousCounterId { get; set; }
        public long converToLong { get; set; }
        private string ReportByServiceIdUrl => $"report/{CounterId}";
        private MarkupString transferMessage => (MarkupString)Message;
        private List<Queue> listClientWaitingOnQueue = new List<Queue>();
        private List<Queue> listClientOnQueueByCounterId = new List<Queue>();
        public enum QueueStatus
        {
            Waiting,
            Completed,
            Missed
        }
        //private HubConnection hubConnection;
        private string Message { get; set; }
        private int currentQueueIndex = 0;
        private Queue? clientOnQueue;
        private Queue currentClient = new Queue();
        private Counter counters = new Counter();
        private PagingParameters paging = new PagingParameters();
        private List<Counter> listCounters = new List<Counter>();
        private List<Counter> listCounterByServiceId = new List<Counter>();
        private List<Service> listServices = new List<Service>();
        private List<Queue> listClientOnQueue = new List<Queue>();
        private List<Queue> listClientWaiting = new List<Queue>();
        private List<Queue> selectedClients = new List<Queue>();
        private List<TimeSpan> serviceTimes = new List<TimeSpan>();
        private DateTime? StartProcessingTime { get; set; }
        private DateTime? EndProcessingTime { get; set; }
        private bool isConnectionDisposed = false;
        private bool isLoadingData = false;
        private bool isConnected = false;
        private bool isButtonDisabled = false;
        private bool isButtonCancelDisabled = false;
        private bool isButtonCallAgainDisabled = false;
        private bool isDeleteConfirmModal = false;
        private bool isTransferModal = false;
        private bool isTransferConfirmModal = false;
        private bool ListTransferConfirm = false;
        private bool isListTrasnferVisible = false;
        private bool selectAll = false;
        //private bool isPreviousCounterId = false;
        // Filtered list of clients based on status
        private List<Queue>? filteredQueue;
        // Bindable property to hold the selected status
        private QueueStatus selectedStatus = QueueStatus.Waiting;
        // Filter method based on status
        private void FilterClients()
        {
            filteredQueue = selectedStatus switch
            {
                QueueStatus.Waiting => listClientOnQueue.Where(c => c.Status == "Waiting").ToList(),
                QueueStatus.Completed => listClientOnQueueByCounterId.Where(c => c.Status == "Completed").ToList(),
                QueueStatus.Missed => listClientOnQueueByCounterId.Where(c => c.Status == "Missed").ToList(),
                _ => new List<Queue>()
            };
            StateHasChanged();
        }
        // Method to change the status and filter clients
        private void ChangeStatus(QueueStatus status)
        {
            selectedStatus = status;
            FilterClients();
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                counters = await CounterApiClient.GetCounterById(CounterId); //Lấy Counter theo CounterID được gán của User
                // Đợi SignalR kết nối xong rồi mới tiếp tục
                //await EnsureSignalRConnected();
                await GetServiceIdByCounterId(); //Lấy thông tin dịch vụ của Counter
                //Lấy dữ liệu ban đầu
                await LoadCounterDataAsync();

                await GetListClientOnQueue();
                //await GetListClientWaiting();
                await GetListServices();
                await GetListClientOnQueueByCounterId();
                var pagedListResponse = await CounterApiClient.GetAllCounter(paging);
                listCounters = pagedListResponse.Items;
                if (serviceTimes == null || !serviceTimes.Any())
                {
                    await InitializeServiceTimesAsync();
                }


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
                await LoadCounterDataAsync();

                await GetListClientOnQueue();
                //await GetListClientWaiting();
                await GetListServices();
                await GetListClientOnQueueByCounterId();
                FilterClients();
                await InvokeAsync(StateHasChanged);
                //StateHasChanged();
                //Console.WriteLine("Data loading completed.");
            });

        }

        private int totalCustomers => listClientWaiting.Count;
        //private int selectedCustomersCount => listClientWaiting.Count(c => c.IsSelected);
        //Lấy danh sách Service 
        private async Task GetListServices()
        {
            listServices = await QueueApiClient.GetServiceList();
            StateHasChanged();
        }
        //
        private async Task GetServiceIdByCounterId()
        {
            counters = await CounterApiClient.GetCounterById(CounterId); //Lấy Counter theo CounterID được gán của User
            ServiceId = counters.ServiceID;
        }
        // Xử lý khi chọn ServiceId
        private async Task OnServiceSelected()
        {
            await GetListCounterByServiceId(ServiceId); // Gọi hàm lấy danh sách Counter theo ServiceId
        }
        // Hàm xử lý khi chọn Counter
        private async Task OnCounterSelected()
        {
            if (selectedCounterId != 0 && currentClient != null)
            {
                // Tìm Counter dựa trên selectedCounterId
                selectedCounter = listCounterByServiceId.FirstOrDefault(c => c.Id == selectedCounterId);
                //Console.WriteLine($"{ServiceId}-{selectedCounter.Name}-{selectedCounter.Id}");
                //await TransferVisitor(ServiceId,selectedCounterId, currentClient);
            }
        }
        //Lấy danh sách Counter theo ServiceId
        private async Task GetListCounterByServiceId(long ServiceId)
        {
            if (ServiceId != 0)
            {
                //converToLong = long.Parse(ServiceId);
                listCounterByServiceId = await CounterApiClient.GetListCounterByServiceId(ServiceId);
                StateHasChanged();
            }
        }
        //Lấy dữ liệu của Counter
        private async Task LoadCounterDataAsync()
        {
            
            counters = await CounterApiClient.GetCounterById(CounterId); //Lấy Counter theo CounterID được gán của User
            if (CounterId != 0 && counters.CurrentNumber != 0)
            {
                listClientWaitingOnQueue = listClientOnQueue.Where(x => x.Status == "Waiting").ToList();
                //Hệ thống kiểm tra không có khách hàng chuyển từ quầy khác tới
                if (previousCounterId == 0)
                {
                    //Lấy thông tin khách hàng hiện tại với số đang được phục vụ ở Counter
                    currentClient = await QueueApiClient.GetQueueByOrderNumberAndCounterIdAsync(counters.CurrentNumber, CounterId, 0);
                }


                if (currentClient == null)
                {
                    currentClient = new Queue();
                }
                if (currentClient.Status == "Waiting" && currentClient.OriginalCounterId == 0)
                {
                    //Console.WriteLine($"Status: Waiting {counters.IsSpecial}");
                    await UpdateStatusProcess(currentClient);
                }

            }
        }
        //Lấy danh sách khách hàng ở trong hàng đợi theo tên Counter
        private async Task GetListClientOnQueue()
        {
            
            if (ServiceId != 0)
            {
                // Lấy danh sách khách hàng trong hàng đợi
                listClientOnQueue = await QueueApiClient.GetQueuesByServiceId(ServiceId);

                //Console.WriteLine($"Quầy đặc biệt:{listClientOnQueue.Count}");
                listClientWaitingOnQueue = listClientOnQueue.Where(x => x.Status == "Waiting").ToList();

            }
            else Console.WriteLine("Load client on queue failed");
        }
        //Lấy danh sách khách hàng đã được phục vụ theo quầy
        private async Task GetListClientOnQueueByCounterId()
        {
            //Console.WriteLine($"Get list client on queue by Counter ID {CounterId}");
            if (CounterId != 0)
            {
                listClientOnQueueByCounterId = await QueueApiClient.GetQueuesByCounterIdAsync(CounterId);
                //StateHasChanged();
            }
            else Console.WriteLine("Load client on queue failed");
        }

        //Gọi Số Tiếp Theo
        //private async Task CallNextTicket()
        //{
        //    try
        //    {
        //        var today = DateTime.Today.Date;
        //        isButtonDisabled = true;
        //        var waitingClient = listClientOnQueue.Where(x => x.Status == "Waiting" && x.PrintTime.Date == today).OrderByDescending(x => x.Priority).ToList();
        //        // Bước 1 : Hoàn thành khách hiện tại nếu đang xử lý
        //        if (currentClient != null && currentClient.Status == "Processing")
        //        {
        //            await UpdateStatusCompleted(currentClient);
        //        }
        //        //Xử lý gọi khách hàng 

        //        if (waitingClient != null && waitingClient.Count > 0)
        //        {
        //            currentClient = waitingClient[0];
        //            //StartProcessingTime = DateTime.Now;
        //            //Cập nhật ID quầy đã gọi khách hàng vào phục vụ
        //            currentClient.CounterId = CounterId;
        //            //Update CounterID vào khách hàng gọi
        //            await QueueApiClient.UpdateQueueById(currentClient.Id, currentClient);
        //            //Cập nhật STT của khách hàng tiếp theo lên màn hình
        //            var result = await CounterApiClient.UpdateCallNextNumberAsync(currentClient!.CounterId, currentClient.OrderNumber);
        //            if (result != null)
        //            {
        //                //Tạo chuỗi text để lưu vào hàng chờ phát loa
        //                await CounterApiClient.CreateTextCallCurrentNumber(counters.Name!, counters.Code!, result.CurrentNumber, currentClient.FullName!, currentClient.IdentificationNumber!);
        //                await UpdateStatusProcess(currentClient);
        //                ToastService.ShowSuccess($"Gọi khách hàng tiếp theo thành công!");
        //                await InvokeAsync(StateHasChanged);
        //            }
        //        }
        //        else
        //        {
        //            if (currentClient!.Status == "Processing")
        //            {
        //                await UpdateStatusCompleted(currentClient);
        //            }
        //        }
        //        // Wait for 3 second to unblock button
        //        await Task.Delay(3000);

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Lỗi khi xử lý lượt tiếp theo.: {ex.Message}");
        //        ToastService.ShowError("Lỗi khi xử lý lượt tiếp theo.");
        //    }
        //    finally
        //    {
        //        isButtonDisabled = false;
        //        await InvokeAsync(StateHasChanged);
        //    }
        //}
        private async Task CallNextTicket()
        {
            if (isButtonDisabled) return;

            try
            {
                isButtonDisabled = true;

                var client = await QueueApiClient.CallNextNumberAsync((int)ServiceId, CounterId);

                if (client != null)
                {
                    currentClient = client;
                    await UpdateStatusProcess(currentClient);
                    await CounterApiClient.UpdateCallNextNumberAsync(CounterId, client.OrderNumber);
                    await CounterApiClient.CreateTextCallCurrentNumber(
                        counters.Name!, counters.Code!, client.OrderNumber,
                        client.FullName!, client.IdentificationNumber!
                    );
                    ToastService.ShowSuccess($"Gọi khách hàng tiếp theo thành công");
                }
                else
                {
                    ToastService.ShowInfo("Không còn khách chờ");
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError("Lỗi: " + ex.Message);
            }
            finally
            {
                await Task.Delay(3000);
                isButtonDisabled = false;
                await InvokeAsync(StateHasChanged);
            }
        }
        private async void CallCurrentNumberAgain()
        {
            try
            {
                if (currentClient != null && currentClient.Status == "Processing" && currentClient.OrderNumber != 0)
                {
                    isButtonCallAgainDisabled = true;
                    var success = await CounterApiClient.CreateTextCallCurrentNumber(counters.Name!, counters.Code!, currentClient.OrderNumber, currentClient.FullName!, currentClient.IdentificationNumber!);
                    ToastService.ShowSuccess($"Gọi lại thành công!");
                    //await LoadCounterDataAsync();
                    //await UpdateStatusProcess(currentClient);
                }
                else
                {
                    ToastService.ShowError($"Chỉ số thứ tự có trạng thái Đang Xử Lý mới có thể gọi lại");
                }
                await Task.Delay(3000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing next ticket: {ex.Message}");
            }
            finally
            {
                isButtonCallAgainDisabled = false;
            }
        }
        //Tính thời gian trung bình phục vụ 1 khách hàng
        private TimeSpan CalculateAverageServiceTime()
        {
            if (serviceTimes == null || serviceTimes.Count == 0)
                return TimeSpan.Zero;

            // Tổng thời gian của tất cả các khách hàng đã được phục vụ
            var totalTime = serviceTimes.Aggregate(TimeSpan.Zero, (sum, next) => sum + next);

            // Tính trung bình theo ticks
            var averageTime = TimeSpan.FromTicks(totalTime.Ticks / serviceTimes.Count);

            // Quy đổi thời gian trung bình thành phút và làm tròn lên
            var roundedMinutes = Math.Ceiling(averageTime.TotalMinutes);

            // Trả về giá trị TimeSpan với số phút đã làm tròn
            return TimeSpan.FromMinutes(roundedMinutes);
        }

        

        private async Task InitializeServiceTimesAsync()
        {
            // Lấy thông tin quầy từ API
            var counter = await CounterApiClient.GetCounterById(CounterId);

            // Giả định API trả về AverageServiceTime là kiểu TimeSpan hoặc số phút
            if (counter.AverageServiceTime != TimeSpan.Zero) // Kiểm tra giá trị đặc biệt
            {
                // Thêm giá trị khởi tạo vào danh sách
                serviceTimes.Add(counter.AverageServiceTime);
            }
            else
            {
                Console.WriteLine("Chưa có dữ liệu thời gian trung bình trong cơ sở dữ liệu.");
            }
        }


        //Hàm đổi Priority Client on Queue
        private async void TogglePriority(Queue item)
        {
            try
            {
                item.Priority = item.Priority == false ? true : false;
                await QueueApiClient.UpdateQueueById(item.Id, item);
                await GetListClientOnQueue();
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., logging)
                Console.WriteLine($"Error updating item status: {ex.Message}");
            }
        }
        //Chỉnh sửa khách hàng trong hàng chờ "Missed" quay lại "Waiting"
        private async Task UpdateStatusWaiting(Queue item)
        {
            try
            {
                if (item.Status == "Missed")
                {
                    item.Status = "Waiting";
                    await QueueApiClient.UpdateQueueById(item.Id, item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating item status: {ex.Message}");
            }
        }
        //Hàm đổi Status khi submit "Hoàn Thành"
        private async Task UpdateStatusProcess(Queue item)
        {
            try
            {

                if (item.Status == "Waiting")
                {
                    item.Status = "Processing";
                    await QueueApiClient.UpdateQueueById(item.Id, item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating item status: {ex.Message}");
            }
        }
        //Hàm đổi Status khi submit 
        private async Task UpdateStatusCompleted(Queue item)
        {
            try
            {
                if (item.Status == "Processing")
                {
                    item.Status = "Completed";
                    await QueueApiClient.UpdateQueueById(item.Id, item);
                    //CallNextTicket();
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., logging)
                Console.WriteLine($"Error updating item status: {ex.Message}");
            }
        }
        //Hàm đổi Status = "Missed" - Trạng thái bị qua lượt
        private async Task UpdateStatusMissed(Queue item)
        {
            try
            {
                if (item.Status == "Processing")
                {
                    item.Status = "Missed";
                    await QueueApiClient.UpdateQueueById(item.Id, item);
                    //await CallNextTicket();
                }
                else ToastService.ShowError("Chỉ số thứ tự có trạng thái Đang Xử Lý mới có thể bỏ qua");
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., logging)
                Console.WriteLine($"Error updating item status: {ex.Message}");
            }
        }
        //Hàm đổi Status = "Cancel" 
        private async Task UpdateStatusCancel(Queue item)
        {
            try
            {
                if (item.Status == "Waiting")
                {
                    isButtonCancelDisabled = true;
                    item.Status = "Cancel";
                    item.CounterId = CounterId;
                    await QueueApiClient.UpdateQueueById(item.Id, item);
                    // Wait for 1 second to unblock button
                    await Task.Delay(500);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., logging)
                Console.WriteLine($"Error updating item status: {ex.Message}");
            }
            finally
            {
                isButtonCancelDisabled = false;
            }
        }
        private async void UpdateStatusTransfer(Queue item)
        {
            try
            {
                if (item.Status == "Processing" || item.Status == "Waiting")
                {
                    item.Status = "Transferred";
                    await QueueApiClient.UpdateQueueById(item.Id, item);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., logging)
                Console.WriteLine($"Error updating item status: {ex.Message}");
            }
        }
        //private async Task TransferSingleClient(long ServiceId, int selectedCounter, Queue clientOnQueue)
        //{
        //    await QueueApiClient.UpdateTransferQueueAsync(ServiceId, selectedCounter, clientOnQueue);
        //    UpdateStatusTransfer(clientOnQueue);
        //    CloseConfirmation();
        //}
        //private async Task TransferListClients(long selectedService, int selectedCounter, List<Queue> listClient)
        //{
        //    var result = await QueueApiClient.UpdateTransferQueueAsync(selectedService, selectedCounter, listClient);
        //    if (result != null)
        //    {
        //        foreach (var client in selectedClients)
        //        {
        //            UpdateStatusTransfer(client!);
        //        }

        //    }

        //}
        //private void SelectAllCustomers(ChangeEventArgs e)
        //{
        //    bool isChecked = (bool)e.Value!;
        //    foreach (var customer in listClientWaiting!)
        //    {
        //        customer.IsSelected = isChecked;
        //    }
        //}


        //private void ShowModal()
        //{
        //    isListTrasnferVisible = true;
        //}
        //private void CheckStatusAndShowModal(List<Queue> listWaiting)
        //{
        //    //Console.WriteLine($"sl client{selectedCustomersCount}");
        //    if (selectedCustomersCount != 0)
        //    {
        //        //Message = $"Are you sure you want to transfer {selectedCustomersCount} clients to Counter {selectedCounter.Name} ? This action cannot be undone.";
        //        isTransferModal = true;
        //        ListTransferConfirm = true;

        //    }
        //    else ToastService.ShowError("You must select clients to transfer!");

        //}
        //Trường hợp chuyển hàng chờ với khách hàng đang ở trong trạng thái phục vụ
        //private void CheckStatusAndShowModal(Queue clientOnQueue)
        //{
        //    if (clientOnQueue.Status == "Processing")
        //    {
        //        isTransferModal = true;
        //    }
        //    else ToastService.ShowError("Only customers in progress can be Transferred to other pending waiting");
        //}
        //private void ShowConfirmCounterTransfer()
        //{
        //    if (ServiceId != 0 && selectedCounterId != 0)
        //    {
        //        if (ListTransferConfirm == false)
        //        {
        //            Message = $"Do you want to transfer <strong>{currentClient.FullName}</strong> to <strong>{selectedCounter.Name}</strong> ? <p> <strong>This action cannot be reversed.</strong> </p>";
        //        }
        //        if (ListTransferConfirm)
        //        {
        //            Message = $"Do you want to transfer <strong>{selectedCustomersCount}</strong> client(s) to <strong>{selectedCounter.Name}</strong> ? <p> <strong>This action cannot be reversed.</strong> </p>";
        //        }
        //        isTransferConfirmModal = true;
        //    }
        //    else ToastService.ShowError("You must select Service and Counter to transfer!");
        //}
        private void ShowDeleteConfirmation(Queue client)
        {
            clientOnQueue = client;
            isDeleteConfirmModal = true;
        }
        private void CloseConfirmation()
        {
            isDeleteConfirmModal = false;
            //isTransferModal = false;
            //isTransferConfirmModal = false;
            //selectedCounterId = 0;
            //ServiceId = 0;
            //ListTransferConfirm = false;
            selectedCounter = new Counter();
            StateHasChanged();

        }
        private void OnCancelClicked()
        {
            isListTrasnferVisible = false;
        }

        //private async void TransferConfirmed()
        //{
        //    if (ListTransferConfirm)
        //    {
        //        selectedClients = listClientWaiting.Where(x => x.IsSelected).ToList();
        //        await TransferListClients(ServiceId, selectedCounterId, selectedClients);
        //        CloseConfirmation();
        //    }
        //    else if (ListTransferConfirm == false)
        //    {
        //        await TransferSingleClient(ServiceId, selectedCounterId, currentClient);
        //    }
        //    else ToastService.ShowError("The current client is already at this counter.");

        //}
        private async Task DeleteConfirmed()
        {
            await UpdateStatusCancel(clientOnQueue!);

            // Đóng modal
            isDeleteConfirmModal = false;
            selectedCounter = new Counter();

            // Gọi cập nhật giao diện
            await InvokeAsync(StateHasChanged);
        }
    }
}
