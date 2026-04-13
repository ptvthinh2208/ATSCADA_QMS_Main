
using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;


namespace ATSCADA_Client.Pages.DashBoard
{
    public partial class CustomerHistoryList
    {
        private PagingParameters paging = new PagingParameters();
        private string? DatetimeToday { get; set; } = DateTime.Now.ToString();
        private string SearchBy { get; set; }
        private string currentSortColumn = "Id";

        private bool isSortAscending = true;
        private DateTimeOffset? StartDate { get; set; } = DateTime.Today;
        private DateTimeOffset? EndDate { get; set; } = DateTime.Today.AddDays(1);
        private List<QueueHistory>? listQueueHistories = new List<QueueHistory>();
        private List<QueueHistory> selectedCustomers = new List<QueueHistory>();
        private List<ZnsInfoDto> znsInfos = new List<ZnsInfoDto>();
        public MetaData MetaData { get; set; } = new MetaData();
        private bool isModalSendZNS = false;
        private bool isConfirmModal = false;
        private string? TemplateName { get; set; }
        private string? CustomersSelected { get; set; }
        protected override async Task OnInitializedAsync()
        {
            paging.PageSize = 5;
            await GetAllQueueHistories();
            znsInfos = await GetZnsTemplate();
            TemplateName = "";
        }
        private async Task GetAllQueueHistories()
        {
            var startDateFormat = StartDate!.Value.DateTime;
            var endDateFormat = EndDate!.Value.DateTime;
            var listPagingResponse = await QueueHistoryApiClient.GetAllQueueHistories(paging, startDateFormat, endDateFormat);
            listQueueHistories = listPagingResponse.Items.GroupBy(x => x.PhoneNumber).Select(group => group.First()).ToList();
            MetaData = listPagingResponse.MetaData;
        }
        private async Task<List<ZnsInfoDto>> GetZnsTemplate()
        {
            return await ZnsConfigApiClient.GetZnsInfoAsync();
        }
        private async Task SelectedPage(int page)
        {
            paging.PageNumber = page;
            await GetAllQueueHistories();
        }

        private async void SortByColumn(string column)
        {
            if (currentSortColumn == column)
            {
                isSortAscending = !isSortAscending;
            }
            else
            {
                currentSortColumn = column;
                isSortAscending = true;
            }
            // Update the paging parameters with the new sort order
            paging.SortBy = currentSortColumn;
            paging.SortOrder = isSortAscending ? "asc" : "desc";

            // Reload the sorted data
            await GetAllQueueHistories();
        }

        private async void OnPageSizeChanged(int selectedPageSize)
        {
            await SortByEntries(selectedPageSize);
        }
        private async Task SortByEntries(int number)
        {
            paging.PageSize = number;
            await GetAllQueueHistories();
            StateHasChanged();
        }
        private async void OnSearch(ChangeEventArgs e, string SearchBy)
        {
            paging.SearchTerm = e.Value?.ToString()!;
            paging.SearchBy = SearchBy;
            await GetAllQueueHistories();
            StateHasChanged();
        }
        private async Task ExportToExcel()
        {
            var a = "data";
            var request = new ExportDataRequest<Report>
            {
                EntityType = a,
                //Model = listSmshistory
            };

            byte[] content = await ExportApiClient.ExportToExcel(request);

            // Ensure content is not null before proceeding
            if (content != null && content.Length > 0)
            {
                var fileName = $"{a}.xlsx";
                var fileUrl = await CreateFileUrl(content);
                DownloadFile(fileUrl, fileName);
            }
            else
            {
                // Handle empty content case if needed
                throw new Exception("No data returned");
            }
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
        private void SelectAllCustomers(ChangeEventArgs e)
        {
            bool isChecked = (bool)e.Value!;
            foreach (var customer in listQueueHistories!)
            {
                customer.IsSelected = isChecked;
            }
        }
        private void SendMessagesToSelectedCustomers()
        {
            selectedCustomers = listQueueHistories!.Where(c => c.IsSelected).ToList();
            if (selectedCustomers != null && selectedCustomers.Any())
            {
                isModalSendZNS = true;
                CustomersSelected = selectedCustomers.Count.ToString();
            }
            else ToastService.ShowError("Chưa chọn khách hàng nào.");
        }
        private void ConfirmSendMessages()
        {
            if (selectedCustomers.Any() && TemplateName != "")
            {
                isConfirmModal = true;
            }
            else
            {
                // Thông báo nếu không có khách hàng nào được chọn
                ToastService.ShowError("Vui lòng chọn Template ZNS cần gửi.");
            }
        }
        private async void SendBulkMessages(List<QueueHistory> selectedCustomers)
        {
            await QueueHistoryApiClient.SendZnsForAllCustomers(selectedCustomers, TemplateName!);
            // Logic gửi tin nhắn
            foreach (var customer in selectedCustomers)
            {
                // Gửi tin nhắn cho từng khách hàng
                Console.WriteLine($"Gửi tin nhắn cho: {customer.FullName}");
            }
            ToastService.ShowSuccess($"Gửi tin nhắn cho {CustomersSelected} khách hàng thành công ");
            CloseModal();
        }
        private void CloseModal()
        {
            isConfirmModal = false;
            isModalSendZNS = false;
            StateHasChanged();
        }
    }
}
