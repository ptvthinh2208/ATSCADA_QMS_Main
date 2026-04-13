using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Library.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ATSCADA_Client.Pages.DashBoard
{
    public partial class QueueReportDetail
    {
        [Parameter]
        public int ReportId {  get; set; }
        private List<ReportDetailsByService> listReportDetails = new List<ReportDetailsByService>();

        private PagingParameters paging = new PagingParameters();
        private string currentSortColumn = "Id";
        private bool isSortAscending = true;
        
        
        protected override async Task OnInitializedAsync()
        {
            //paging.PageSize = 5;
            //await GetAllReports();
            //await GetAllQueueHistories();
            //filteredData = listQueueHistories;
            await GetReportDetailById();
            //Thay đổi hiển thị trên thanh đường dẫn
            if(listReportDetails != null)
            {
                var formattedDate = listReportDetails[0].CreatedDate.ToString("ddMMyyyy");
                var newUrl = $"/report-detail/{formattedDate}";
                // Điều hướng đến URL mới
                NavigationManager.NavigateTo(newUrl, replace: true);
            }
        }
        private async Task GetReportDetailById()
        {
            var detailsReport = await ReportApiClient.GetDetailsReportById(ReportId);
            foreach (var report in detailsReport)
            {
                var ServiceName = await QueueApiClient.GetServiceById(report.ServiceId);
                var exportReportDetails = new ReportDetailsByService()
                {
                    ServiceId = report.ServiceId,
                    ServiceName = ServiceName.Description, //Lấy thông tin tên dịch vụ
                    TotalPrint = report.TotalPrint,
                    TotalCompleted = report.TotalCompleted,
                    TotalMissed = report.TotalMissed,
                    TotalCancel = report.TotalCancel,
                    CreatedDate = report.CreatedDate
                };

                // Add the mapped DTO to the list
                listReportDetails.Add(exportReportDetails);
            }
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
            await GetReportDetailById();
        }
        private async Task ExportToExcel<T>(IEnumerable<T> dataList, string filePrefix)
        {
            if (dataList != null && dataList.Any())
            {
                string formattedStartDate = listReportDetails[0].CreatedDate.ToString("ddMMyyyy");
                string formattedEndDate = formattedStartDate;
                var nameFile = $"{filePrefix}_{formattedStartDate}_{formattedEndDate}";
                var request = new ExportDataRequest<T>
                {
                    EntityType = nameFile,
                    Model = dataList
                };

                byte[] content = await ExportApiClient.ExportToExcel(request);

                if (content != null && content.Length > 0)
                {
                    var fileName = $"{nameFile}.xlsx";
                    var fileUrl = await CreateFileUrl(content);
                    DownloadFile(fileUrl, fileName);
                }
                else
                {
                    throw new Exception("No data returned");
                }
            }
            else
            {
                ToastService.ShowError("No data to export");
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
    }
}
