using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Library.Entities;
using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Enums;
using ChartJs.Blazor.Util;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;


//using DocumentFormat.OpenXml.Presentation;
using System.Drawing;
using System.Globalization;

namespace ATSCADA_Client.Components.Chart
{
    public partial class CustomerBarChart
    {
        private BarConfig? _barConfig;
        private PagingParameters paging = new PagingParameters();
        private List<QueueHistory>? listQueueHistories = new List<QueueHistory>();
        private List<int> avgCustomerData = new List<int>();
        
        private DateTime? StartDateOfMonth { get; set; }
        private DateTime? EndDateOfMonth { get; set; }
        private DateTime SelectedMonth { get; set; } = DateTime.Now;
        protected override async Task OnInitializedAsync()
        {
            UpdateMonthRange(SelectedMonth);
            await GetQueueHistories();
            ConfigureBarConfig();
        }

        private async Task GetQueueHistories()
        {
            var listPagingResponse = await QueueHistoryApiClient.GetAllQueueHistories(paging, StartDateOfMonth, EndDateOfMonth);
            listQueueHistories = listPagingResponse.Items;
            await InvokeAsync(StateHasChanged);  // Cập nhật lại giao diện
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
                await GetQueueHistories();
                ConfigureBarConfig();
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

        private List<int> GetAvgCustomerByHour()
        {
            var hours = Enumerable.Range(8, 10).ToList();

            if (listQueueHistories == null || !listQueueHistories.Any())
            {
                return new List<int>();
            }

            var customerCountByHour = listQueueHistories
                .Where(h => h.PrintTime.TimeOfDay >= TimeSpan.FromHours(8) && h.PrintTime.TimeOfDay < TimeSpan.FromHours(17))
                .GroupBy(h => h.PrintTime.Hour)
                .ToDictionary(g => g.Key, g => g.Count());

            return hours
                .Select(hour => customerCountByHour.ContainsKey(hour) ? customerCountByHour[hour] : 0)
                .ToList();
        }
        private async Task DownloadChart()
        {
            await JSRuntime.InvokeVoidAsync("downloadChartAsImage", "chartContainer", $"BarChart_{StartDateOfMonth!.Value.ToString("MM/yyyy")}.png");
        }

        private void ConfigureBarConfig()
        {
            if (StartDateOfMonth == null) return;

            // Định dạng StartDateOfMonth cho tiêu đề
            var formattedDate = StartDateOfMonth.Value.ToString("MM/yyyy");

            avgCustomerData = GetAvgCustomerByHour();

            List<string> timeLabels = new List<string>();
            TimeSpan startTime = new TimeSpan(8, 0, 0);
            TimeSpan endTime = new TimeSpan(17, 0, 0);
            TimeSpan interval = new TimeSpan(1, 0, 0);

            // Initialize _barConfig if not already set
            if (_barConfig == null)
            {
                
                _barConfig = new BarConfig()
                {
                    Options = new BarOptions
                    {
                        Responsive = true,
                        Legend = new Legend { Position = Position.Top },
                        Title = new OptionsTitle
                        {
                            Display = true,
                            Text = $"Average customer bookings by time slot on {formattedDate} "
                        },
                    },
                    
                };
            }
            else
            {
                // Nếu _barConfig đã tồn tại, chỉ cần cập nhật tiêu đề
                _barConfig.Options.Title.Text = $"Average customer bookings by time slot on {formattedDate}";
            }
            // Fill timeLabels for X-axis labels
            for (TimeSpan time = startTime; time <= endTime; time += interval)
            {
                timeLabels.Add(DateTime.Today.Add(time).ToString("HH:mm"));
            }

            // Clear Label chart cũ và set Label chart mới
            _barConfig.Data.Labels.Clear();
            foreach (var timeLabel in timeLabels)
            {
                _barConfig.Data.Labels.Add(timeLabel);
            }

            // Clear and set dataset for chart data
            _barConfig.Data.Datasets.Clear();
            var dataset1 = new BarDataset<int>(avgCustomerData)
            {
                Label = "Customer Booking",
                BackgroundColor = ColorUtil.FromDrawingColor(Color.FromArgb(128, System.Drawing.Color.Red)),
                BorderColor = ColorUtil.FromDrawingColor(System.Drawing.Color.Red),
                BorderWidth = 1,
                
            };
            _barConfig.Data.Datasets.Add(dataset1);

            // Call StateHasChanged to refresh component state
            StateHasChanged();
        }

    }
    
}
