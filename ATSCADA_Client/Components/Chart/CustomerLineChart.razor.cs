using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Enums;
using ChartJs.Blazor.LineChart;
using ChartJs.Blazor.Util;
using System.Drawing;

namespace ATSCADA_Client.Components.Chart
{
    public partial class CustomerLineChart
    {
        private LineConfig? _lineConfig;
        private DateTime StartDateOfWeek { get; set; }
        private DateTime EndDateOfWeek { get; set; }
        protected override void OnInitialized()
        {
            SetStartAndEndOfWeekDates();
            ConfigureLineConfig();
        }
        private void ConfigureLineConfig()
        {
            List<string> timeLabels = new List<string>();
            //TimeSpan startTime = new TimeSpan(8, 0, 0); // 8:00 AM
            //TimeSpan endTime = new TimeSpan(17, 0, 0);  // 5:00 PM
            TimeSpan interval = new TimeSpan(1, 0, 0, 0);  // 1 ngày mỗi lần
            var customerData = new List<int> { 5, 10, 15, 7, 20, 25, 30 }; // Số liệu tùy chọn

            _lineConfig = new LineConfig
            {
                Options = new LineOptions
                {
                    Responsive = true,
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = "ChartJs.Blazor Line Chart"
                    },
                    Tooltips = new Tooltips
                    {
                        Mode = InteractionMode.Nearest,
                        Intersect = true
                    },
                    Hover = new Hover
                    {
                        Mode = InteractionMode.Nearest,
                        Intersect = true
                    },
                    Scales = new Scales
                    {
                        XAxes = new List<CartesianAxis>
                    {
                        new CategoryAxis
                        {
                            ScaleLabel = new ScaleLabel
                            {
                                LabelString = "Month"
                            }
                        }
                    },
                        YAxes = new List<CartesianAxis>
                    {
                        new LinearCartesianAxis
                        {
                            ScaleLabel = new ScaleLabel
                            {
                                LabelString = "Value"
                            }
                        }
                    }
                    }
                }
            };
            IDataset<int> dataset1 = new LineDataset<int>(customerData)
            {
                Label = "Customer Booking",
                BackgroundColor = ColorUtil.FromDrawingColor(Color.FromArgb(128, System.Drawing.Color.BlueViolet)),
                BorderColor = ColorUtil.FromDrawingColor(System.Drawing.Color.BlueViolet),
                Fill = FillingMode.Disabled
            };
            for (DateTime time = StartDateOfWeek; time <= EndDateOfWeek; time += interval)
            {
                timeLabels.Add(time.ToString("dd/MM/yyyy")); // Định dạng hh:mm AM/PM
            }
            foreach (var time in timeLabels)
            {
                _lineConfig.Data.Labels.Add(time);
            }
            _lineConfig.Data.Datasets.Add(dataset1);

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
    }
}
