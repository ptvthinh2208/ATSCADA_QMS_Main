using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Library.Entities;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.PieChart;
using ChartJs.Blazor.Util;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace ATSCADA_Client.Components.Chart
{
    public partial class CustomerPieChart
    {
        //Chart configuration
        private PieConfig? _pieConfig;
        private PagingParameters paging = new PagingParameters();
        private List<Feedback>? listFeedbacks = new List<Feedback>();
        private DateTime? StartDateOfMonth { get; set; }
        private DateTime? EndDateOfMonth { get; set; }
        private DateTime SelectedMonth { get; set; } = DateTime.Now;
        protected override async Task OnInitializedAsync()
        {
            UpdateMonthRange(SelectedMonth);
            await GetFeedbacks();
            ConfigurePieConfig();
        }
        private async Task GetFeedbacks()
        {
            var listPagingResponse = await FeedbackApiClient.GetAllFeedback(paging, StartDateOfMonth, EndDateOfMonth);
            listFeedbacks = listPagingResponse.Items;

        }
        private void UpdateMonthRange(DateTime month)
        {
            StartDateOfMonth = new DateTime(month.Year, month.Month, 1);
            EndDateOfMonth = StartDateOfMonth.Value.AddMonths(1).AddSeconds(-1);
        }

        private async Task OnMonthChanged(ChangeEventArgs e)
        {
            if (DateTime.TryParseExact(e.Value?.ToString(), "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var selectedMonth))
            {
                SelectedMonth = selectedMonth;
                UpdateMonthRange(SelectedMonth);
                await GetFeedbacks();
                ConfigurePieConfig();
            }
        }

        private IEnumerable<DateTime> GetMonthList()
        {
            var months = new List<DateTime>();
            var currentDate = DateTime.Now;

            for (int i = 0; i < 12; i++)
            {
                months.Add(currentDate.AddMonths(-i));
            }

            return months;
        }

        public List<int> CalculateFeedbackCounts(List<Feedback> feedbacks)
        {
            var ratingCounts = new List<int>();

            // Tính tổng số lượng đánh giá cho mỗi mức điểm từ 1 đến 5
            for (int i = 1; i <= 5; i++)
            {
                int count = feedbacks.Count(f => f.Rating == i);
                ratingCounts.Add(count);
            }

            return ratingCounts;
        }
        private async Task DownloadChart()
        {
            await JSRuntime.InvokeVoidAsync("downloadChartAsImage", "chartContainer_Pie", $"PieChart_{StartDateOfMonth!.Value.ToString("MM/yyyy")}.png");
        }
        private void ConfigurePieConfig()
        {
            // Định dạng StartDateOfMonth cho tiêu đề
            var formattedDate = StartDateOfMonth!.Value.ToString("MM/yyyy");

            var feedbackCounts = CalculateFeedbackCounts(listFeedbacks!);

            // Initialize _barConfig if not already set
            if (_pieConfig == null)
            {

                _pieConfig = new PieConfig()
                {
                    Options = new PieOptions
                    {
                        Responsive = true,
                        Title = new OptionsTitle
                        {
                            Display = true,
                            Text = $"Rating of all counters on {formattedDate} (★)"
                        }
                    },

                };
            }
            else
            {
                _pieConfig.Options.Title.Text = $"Rating of all counters on {formattedDate} (★)";
            }
            // Clear Label chart cũ và set Label chart mới
            _pieConfig.Data.Labels.Clear();
            foreach (string color in new[] { "1", "2","3", "4", "5" })
            {
                _pieConfig.Data.Labels.Add($"{color}★");
            }
            // Clear and set dataset for chart data
            _pieConfig.Data.Datasets.Clear();

            PieDataset<int> dataset = new PieDataset<int>(feedbackCounts)
            {
                BackgroundColor = new[]
                {
            ColorUtil.ColorHexString(255, 99, 132), // Slice 1 aka "Red"
            ColorUtil.ColorHexString(255, 205, 86), // Slice 2 aka "Yellow"
            ColorUtil.ColorHexString(102, 0, 255), //Purple
            ColorUtil.ColorHexString(75, 192, 192), // Slice 3 aka "Green"
            ColorUtil.ColorHexString(54, 162, 235), // Slice 4 aka "Blue"
                }
            };

            _pieConfig.Data.Datasets.Add(dataset);
        }

    }
}
