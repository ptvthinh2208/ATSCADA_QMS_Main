using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ATSCADA_Client.Pages.DashBoard
{
    public partial class QueueReportList
    {
        private List<Report>? listReport = new List<Report>();
        private List<Report>? listDetailedReport = new List<Report>();
        private List<Service>? availableServices = new List<Service>();
        private PagingParameters paging = new PagingParameters();
        private List<QueueHistory> listQueueHistories = new List<QueueHistory>();
        private List<Counter> listCounterByServiceId = new List<Counter>();
        private Dictionary<string, int> queueStatistics = new();
        private Dictionary<long, Dictionary<string, int>> queueStatisticsByServiceId = new();
        private List<QueueHistory> filteredData;
        private string currentSortColumn = "Id";
        private bool isSortAscending = true;
        private string? DatetimeToday { get; set; } = DateTime.Now.ToString();
        private DateTimeOffset? StartDate { get; set; } = DateTime.Today;
        private DateTimeOffset? EndDate { get; set; } = DateTime.Today.AddDays(1);
        public MetaData MetaData { get; set; } = new MetaData();
        //Chart
        private bool isPerformanceModalVisible = false;
        private bool isPerformanceServiceModalVisible = false;
        private string activeTab = "TotalPrint";
        private string chartColor = "rgba(75, 192, 192, 0.2)";  // Màu mặc định
        private List<string> dateLabels = new List<string>();
        private List<int?> dataCharts = new List<int?>();
        // Dữ liệu mẫu - ngày có số liệu
        Dictionary<DateTime, int> rawData { get; set; }
        // Danh sách các báo cáo với thống kê theo từng dịch vụ và ngày
        Dictionary<string, Dictionary<DateTime, int>> detailedReports { get; set; }
        protected override async Task OnInitializedAsync()
        {
            paging.PageSize = 5;
            await GetAllReports();
            await GetAllQueueHistories();
            await GetAllService();

            filteredData = listQueueHistories;
        }
        
        private async Task btnReport()
        {
            await GetAllReports();
            await GetAllQueueHistories();
        }
        private async Task GetAllReports()
        {
            var startDateFormat = StartDate!.Value.DateTime;
            var endDateFormat = EndDate!.Value.DateTime;
            var listPagingResponse = await ReportApiClient.GetAllReports(paging, startDateFormat, endDateFormat);
            listReport = listPagingResponse.Items;
            MetaData = listPagingResponse.MetaData;
        }
        private async Task GetAllService()
        {
            availableServices = await QueueApiClient.GetServiceList();
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
        private async Task GetAllQueueHistories()
        {
            var pagingAll = new PagingParameters();
            var startDateFormat = StartDate!.Value.DateTime;
            var endDateFormat = EndDate!.Value.DateTime;
            var listPagingResponse = await QueueHistoryApiClient.GetAllQueueHistories(pagingAll, startDateFormat, endDateFormat);
            listQueueHistories = listPagingResponse.Items;
        }
        
        private async Task SelectedPage(int page)
        {
            paging.PageNumber = page;
            await btnReport();
        }
        private async void OnSearch(ChangeEventArgs e)
        {
            paging.SearchTerm = e.Value?.ToString()!;
            await btnReport();
            StateHasChanged();
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
            await GetAllReports();
        }
        private async void OnPageSizeChanged(int selectedPageSize)
        {
            await SortByEntries(selectedPageSize);
        }
        private async Task SortByEntries(int number)
        {
            paging.PageSize = number;
            await btnReport();
            StateHasChanged();
        }
        //Chuyển hướng sang trang report-detail
        private void ExportQueueDetail(int reportId)
        {
            NavigationManager.NavigateTo($"/report-detail/{reportId}");
        }
        private async Task ExportToExcel<T>(IEnumerable<T> dataList, string filePrefix)
        {
            if (dataList != null && dataList.Any())
            {
                string formattedStartDate = StartDate!.Value.DateTime.ToString("ddMMyyyy");
                string formattedEndDate = EndDate!.Value.DateTime.ToString("ddMMyyyy");
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
        private async Task ExportDetailToExcel<T>(IEnumerable<T> dataList, string filePrefix) where T : QueueHistory
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
                        Status = qh.Status == "Processing" ? "Completed" : qh.Status,
                        LastTimeUpdated = qh.LastTimeUpdated
                    };
                }).ToList();

                string formattedStartDate = StartDate!.Value.DateTime.ToString("ddMMyyyy");
                string formattedEndDate = EndDate!.Value.DateTime.ToString("ddMMyyyy");
                var nameFile = $"{filePrefix}_{formattedStartDate}_{formattedEndDate}";
                if (SelectedService != 0)
                {
                    var nameService = await QueueApiClient.GetServiceById(SelectedService);
                    nameFile = $"{filePrefix}_{nameService.Description}_{formattedStartDate}_{formattedEndDate}";
                }

                var request = new ExportDataRequest<QueueHistoryToExportData>
                {
                    EntityType = nameFile,
                    Model = listDataExport
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
        //Chart 

        private void PrepareChartData(DateTime start, DateTime end, Dictionary<DateTime, int> rawData)
        {
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                dateLabels.Add(date.ToString("dd-MM-yyyy")); // Tạo nhãn cho mỗi ngày trong khoảng thời gian
                dataCharts.Add(rawData.ContainsKey(date) ? rawData[date] : (int?)null); // Thêm null nếu không có dữ liệu cho ngày đó
            }
        }

        private Dictionary<DateTime, int> GetRawData(string typeDataReport)
        {
            rawData = new Dictionary<DateTime, int>();
            foreach (var report in listReport!)
            {
                int value;
                // Chọn giá trị phù hợp dựa trên typeDataReport
                switch (typeDataReport)
                {
                    case "TotalPrint":
                        value = report.TotalPrint;
                        break;
                    case "TotalCompleted":
                        value = report.TotalCompleted;
                        break;
                    case "TotalCancel":
                        value = report.TotalCancel;
                        break;
                    default:
                        throw new ArgumentException("Invalid report type");
                }
                rawData[report.CreatedDate] = value;
            }
            return rawData;
        }
        private void ShowPerformanceModal()
        {
            isPerformanceModalVisible = true;
            // Xóa dữ liệu cũ
            dateLabels.Clear();
            dataCharts.Clear();
            // Lấy dữ liệu mới và chuẩn bị cho biểu đồ
            GetRawData(activeTab);
            PrepareChartData(StartDate.Value.DateTime, EndDate.Value.DateTime, rawData);
            // Gọi StateHasChanged để cập nhật lại giao diện
            StateHasChanged();
        }
        private void ShowPerformanceServiceModal()
        {
            isPerformanceServiceModalVisible = true;
            StateHasChanged();
        }
        private void HidePerformanceModal()
        {
            SelectedService = 0;
            isPerformanceModalVisible = false;
            isPerformanceServiceModalVisible = false;
        }

        // Phương thức thay đổi màu sắc tùy theo tab
        private void SetChartColor()
        {
            switch (activeTab)
            {
                case "TotalPrint":
                    chartColor = "rgba(255, 99, 132, 0.2)";  // Màu cho TotalPrint
                    break;
                case "TotalCompleted":
                    chartColor = "rgba(54, 162, 235, 0.2)";  // Màu cho TotalCompleted
                    break;
                case "TotalCancel":
                    chartColor = "rgba(255, 159, 64, 0.2)";  // Màu cho TotalCancel
                    break;
                default:
                    chartColor = "rgba(75, 192, 192, 0.2)";  // Màu mặc định
                    break;
            }
        }
        private void SwitchTab(string tab)
        {
            // Cập nhật màu sắc khi chuyển tab
            activeTab = tab;
            SetChartColor();
            // Xóa dữ liệu cũ mỗi khi chuyển tab
            dateLabels.Clear();
            dataCharts.Clear();
            // Lọc dữ liệu mới dựa trên tab đã chọn
            switch (tab)
            {
                case "TotalCancel":
                    filteredData = listQueueHistories.Where(q => q.Status != "Completed").ToList();
                    break;
                case "TotalCompleted":
                    filteredData = listQueueHistories.Where(q => q.Status == "Completed").ToList();
                    break;
                // Thêm các case cho các tab khác nếu cần thiết
                default:
                    filteredData = listQueueHistories;
                    break;
            }
            //Console.WriteLine($"Data:{filteredData.Count}");
            // Cập nhật dữ liệu mới theo tab đã chọn
            GetRawData(tab);
            PrepareChartData(StartDate.Value.DateTime, EndDate.Value.DateTime, rawData);

            // Gọi StateHasChanged để cập nhật lại giao diện
            StateHasChanged();
        }
    }
}
