using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ATSCADA_Client.Pages.DashBoard
{
    public partial class FeedbackList
    {
        private List<Feedback>? listFeedback = new List<Feedback>();
        private PagingParameters paging = new PagingParameters();
        private string SearchBy { get; set; }

        private string currentSortColumn = "Id";
        private bool isSortAscending = true;
        private string? DatetimeToday { get; set; } = DateTime.Now.ToString();
        private DateTimeOffset? StartDate { get; set; } = DateTime.Today;
        private DateTimeOffset? EndDate { get; set; } = DateTime.Today.AddDays(1);
        public MetaData MetaData { get; set; } = new MetaData();
        protected override async Task OnInitializedAsync()
        {
            paging.PageSize = 5;
            await GetAllFeedbacks();
        }
        private async Task GetAllFeedbacks()
        {
            
            var startDateFormat = StartDate!.Value.DateTime;
            var endDateFormat = EndDate!.Value.DateTime;
            var listPagingResponse = await FeedbackApiClient.GetAllFeedback(paging, startDateFormat, endDateFormat);
            listFeedback = listPagingResponse!.Items;
            MetaData = listPagingResponse.MetaData;
        }
        private async Task SelectedPage(int page)
        {
            paging.PageNumber = page;
            await GetAllFeedbacks();
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
            await GetAllFeedbacks();
        }
        private async void OnPageSizeChanged(int selectedPageSize)
        {
            await SortByEntries(selectedPageSize);
        }
        private async Task SortByEntries(int number)
        {
            paging.PageSize = number;
            await GetAllFeedbacks();
            StateHasChanged();
        }
        private async void OnSearch(ChangeEventArgs e, string SearchBy)
        {
            paging.SearchTerm = e.Value?.ToString()!;
            paging.SearchBy = SearchBy;
            await GetAllFeedbacks();
            StateHasChanged();
        }
        //private async void OnSearch(ChangeEventArgs e)
        //{
        //    paging.SearchTerm = e.Value?.ToString()!;
        //    await GetAllFeedbacks();
        //    StateHasChanged();
        //}
        private async Task ExportToExcel()
        {
            if (listFeedback != null && listFeedback.Any())
            {
                var startDateFormat = StartDate!.Value.DateTime;
                var endDateFormat = EndDate!.Value.DateTime;
                var nameFile = $"Feedback_Report_{startDateFormat}_{endDateFormat}";
                var request = new ExportDataRequest<Feedback>
                {
                    EntityType = nameFile,
                    Model = listFeedback
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
    }
}
