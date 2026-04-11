using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ClosedXML.Excel;
using OfficeOpenXml;
using System.Reflection;

namespace ATSCADA_Library.Helpers.ExportFile
{
    public class ExcelExport
    {
        private string GenerateSheetName(string originalName)
        {
            // Loại bỏ ký tự không hợp lệ
            var invalidChars = new[] { ':', '\\', '/', '?', '*', '[', ']' };
            string sanitized = new string(originalName
                .Where(c => !invalidChars.Contains(c))
                .ToArray());

            // Giới hạn độ dài tối đa 31 ký tự
            if (sanitized.Length > 31)
            {
                sanitized = sanitized.Substring(0, 31);
            }

            // Trả về tên sheet đã xử lý
            return sanitized;
        }

        public byte[] ExportToExcel<T>(IEnumerable<T> dataList, string worksheetName) where T : new()
        {
            // Create Excel using ClosedXML
            using (var closedXmlStream = new MemoryStream())
            {
                string sheetName = GenerateSheetName(worksheetName);
                int numberOfColumns;
                var data = dataList.ToList(); //Chuyển IEnumerable thành List
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(sheetName);
                    if (data == null || data.Count == 0)
                    {
                        throw new ArgumentException("The list is empty.");
                    }

                    // Lấy các thuộc tính không có attribute IgnoreExport
                    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                              .Where(p => !Attribute.IsDefined(p, typeof(IgnoreExportAttribute)))
                                              .ToArray();
                    numberOfColumns = properties.Length;

                    // Thêm tiêu đề báo cáo ở đầu trang tính
                    var titleCell = worksheet.Cell(1, 1);
                    titleCell.Value = $"{worksheetName}";
                    titleCell.Style.Font.Bold = true;
                    titleCell.Style.Font.FontColor = XLColor.Red;
                    titleCell.Style.Font.FontSize = 16;
                    worksheet.Range(1, 1, 1, numberOfColumns).Merge(); // Merge cells for title
                    titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Lấy ngày bắt đầu và ngày kết thúc dựa trên CreatedDate
                    var reportDateProperty = properties.FirstOrDefault(p => p.Name == "CreatedDate" || p.Name == "PrintTime");
                    if (reportDateProperty == null)
                    {
                        throw new Exception("No 'CreatedDate' property found.");
                    }
                    var startDate = reportDateProperty.GetValue(data.First())?.ToString();
                    var endDate = reportDateProperty.GetValue(data.Last())?.ToString();
                    var count = data.Count;
                    // Thêm ngày bắt đầu và ngày kết thúc
                    worksheet.Cell(2, 1).Value = "Start Date:";
                    worksheet.Cell(2, 1).Style.Font.Bold = true;
                    worksheet.Cell(2, 1).Style.Font.FontColor = XLColor.Red;
                    worksheet.Cell(2, 2).Value = startDate;
                    worksheet.Cell(3, 1).Value = "End Date:";
                    worksheet.Cell(3, 1).Style.Font.Bold = true;
                    worksheet.Cell(3, 1).Style.Font.FontColor = XLColor.Red;
                    worksheet.Cell(3, 2).Value = endDate;
                    //
                    worksheet.Cell(3, numberOfColumns - 1).Value = "Total Data Count:";
                    worksheet.Cell(3, numberOfColumns - 1).Style.Font.Bold = true;
                    worksheet.Cell(3, numberOfColumns - 1).Style.Font.FontColor = XLColor.Red;
                    worksheet.Cell(3, numberOfColumns).Value = count;


                    // Define colors for the columns (bạn có thể thêm nhiều màu hơn nếu cần)
                    var columnColors = new[]
                    {
                XLColor.White,
                XLColor.FromTheme(XLThemeColor.Accent1, 0.6),
                XLColor.FromTheme(XLThemeColor.Accent2, 0.6),
                XLColor.FromTheme(XLThemeColor.Accent3, 0.6),
                XLColor.FromTheme(XLThemeColor.Accent4, 0.6),
                XLColor.FromTheme(XLThemeColor.Accent5, 0.6),
                XLColor.FromTheme(XLThemeColor.Accent6, 0.6)
            };

                    // Set header with styles (dòng thứ 5 bắt đầu header dữ liệu)
                    for (int i = 0; i < properties.Length; i++)
                    {
                        var headerCell = worksheet.Cell(5, i + 1);
                        headerCell.Value = properties[i].Name;

                        // Apply styles for header and background color for each column
                        headerCell.Style.Font.Bold = true;
                        headerCell.Style.Font.FontSize = 14;
                        headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Apply background color to header based on column index
                        int colorIndex = i % columnColors.Length; // Tính chỉ số màu cho mỗi cột
                        headerCell.Style.Fill.BackgroundColor = columnColors[colorIndex];
                        // Apply border style for header
                        headerCell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        headerCell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        headerCell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        headerCell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    }

                    // Populate data and style columns (dữ liệu bắt đầu từ dòng 6)
                    for (int row = 0; row < data.Count; row++)
                    {
                        for (int col = 0; col < properties.Length; col++)
                        {
                            var value = properties[col].GetValue(data[row]);
                            var cell = worksheet.Cell(row + 6, col + 1); // Dòng thứ 6 trở đi

                            // Kiểm tra kiểu dữ liệu và gán giá trị một cách rõ ràng
                            if (value is int || value is double || value is float || value is decimal)
                            {
                                // Gán giá trị kiểu số nếu dữ liệu là kiểu số
                                cell.Value = Convert.ToDouble(value); // Chuyển đổi về kiểu double
                            }
                            else if (value is DateTime)
                            {
                                // Gán giá trị kiểu ngày tháng nếu dữ liệu là DateTime
                                cell.Value = (DateTime)value;
                            }
                            else
                            {
                                // Gán giá trị chuỗi nếu không phải là số hay ngày tháng
                                cell.Value = value != null ? value.ToString() : string.Empty;
                            }

                            // Apply background color for each column based on the column index
                            int colorIndex = col % columnColors.Length;
                            cell.Style.Fill.BackgroundColor = columnColors[colorIndex];
                            // Apply border style for each cell
                            cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        }
                    }

                    // Tự động điều chỉnh độ rộng các cột
                    worksheet.Columns().AdjustToContents();

                    // Save the ClosedXML workbook to a memory stream
                    workbook.SaveAs(closedXmlStream);
                }

                // Chỉ vẽ biểu đồ khi `Entity` là `Feedback`, `Report`, hoặc `ReportDetailsByService`
                if (typeof(T) == typeof(Report) || typeof(T) == typeof(ReportDetailsByService))
                {
                    using (var epPlusStream = new MemoryStream(closedXmlStream.ToArray()))
                    {
                        using (var package = new ExcelPackage(epPlusStream))
                        {
                            var excelWorksheet = package.Workbook.Worksheets[worksheetName];

                            // Group and chart creation logic for each of these types as needed
                           
                            if (typeof(T) == typeof(Report) || typeof(T) == typeof(ReportDetailsByService))
                            {
                                // Create chart logic for Report and ReportDetailsByService
                                var chartWorksheet = package.Workbook.Worksheets.Add("ChartSheet");

                                var chart = chartWorksheet.Drawings.AddChart("barChart", OfficeOpenXml.Drawing.Chart.eChartType.ColumnClustered);
                                chart.Title.Text = "DATA REPORT";
                                chart.SetPosition(5, 0, 1, 0);
                                chart.SetSize(600, 400);

                                var xAxisRange = excelWorksheet.Cells[6, 1, data.Count + 5, 1];
                                for (int col = 2; col <= numberOfColumns; col++)
                                {
                                    var series = chart.Series.Add(excelWorksheet.Cells[6, col, data.Count + 5, col], xAxisRange);
                                    series.Header = excelWorksheet.Cells[5, col].Value.ToString();
                                }

                                return package.GetAsByteArray();
                            }
                        }
                    }
                }
                // Nếu không cần vẽ biểu đồ, trả về Excel ngay lập tức
                return closedXmlStream.ToArray();
            }
        }
    }
}
