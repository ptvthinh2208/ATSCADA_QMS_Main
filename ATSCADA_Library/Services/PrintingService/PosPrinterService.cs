using ATSCADA_Library.DTOs;
using CallQueue.Core;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace ATSCADA_Library.Services.PrintingService
{
    public class PosPrinterService
    {
        private readonly string printerIp;
        private readonly int printerPort;

        public PosPrinterService(string ip, int port = 9100)
        {
            printerIp = ip;
            printerPort = port;
        }
        public static string RemoveDiacritics(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string normalizedString = input.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            string result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            // Bổ sung thay thế ký tự đ/Đ
            result = result.Replace('đ', 'd').Replace('Đ', 'D');

            return result;
        }

        public void PrintForm(List<PrintTemplate> formItems, PrintRequest request, string date, string time)
        {
            try
            {
                MemoryStream ms = new MemoryStream();

                // 1. Khởi tạo máy in
                ms.Write(new byte[] { 0x1B, 0x40 }, 0, 2);

                foreach (var line in formItems)
                {

                    // Bỏ qua nếu dòng trống, chỉ xuống hàng
                    if (string.IsNullOrWhiteSpace(line.Text))
                    {
                        ms.Write(new byte[] { 0x0A }, 0, 1);
                        continue;
                    }

                    // Thay thế các tag trong text
                    string text = line.Text.Replace("{1}", request.TicketNumber)
                                           //.Replace("{2}", request.Counter)
                                           .Replace("{3}", request.ServiceName)
                                           .Replace("{Date}", date)
                                           .Replace("{Time}", time);

                    // 2. Tạo ảnh từ text
                    // Tùy chỉnh Font và cỡ chữ ở đây. Cỡ chữ có thể nhân với 10 hoặc 12 để dễ nhìn.

                    // Tăng cỡ chữ cơ bản và mức độ nhân lên để chữ to và rõ hơn
                    int fontSize = (int)line.HeightSize * 10 + 16; // Ví dụ: Cỡ 1 -> 16px, Cỡ 2 -> 30px

                    // Xác định kiểu chữ (đậm, thường) dựa trên PrintMode
                    FontStyle fontStyle = FontStyle.Regular;
                    if (line.PrintMode == PrintMode.Bolder) // Giả sử bạn có enum PrintMode.Bold
                    {
                        fontStyle = FontStyle.Bold;
                    }

                    // Tạo font với các thuộc tính đã xác định
                    Font font = new Font("Arial", fontSize, fontStyle, GraphicsUnit.Pixel);

                    // --- KẾT THÚC THAY ĐỔI ---

                    // Xác định căn lề dựa trên JustifyMode
                    bool isCenter = line.Justify == JustifyMode.Center;

                    // Tạo ảnh bitmap từ chuỗi text
                    using (Bitmap bitmap = CreateBitmapFromText(text, font, isCenter))
                    {
                        // 3. Chuyển ảnh thành dữ liệu byte và ghi vào stream
                        byte[] imageData = ConvertBitmapToPrinterBytes(bitmap);
                        ms.Write(imageData, 0, imageData.Length);
                    }
                }

                // 4. Cắt giấy
                ms.Write(new byte[] { 0x1D, (byte)'V', 65, 1 }, 0, 4);

                // 5. Gửi toàn bộ dữ liệu tới máy in 

                if (!string.IsNullOrWhiteSpace(request.PrinterIP))
                {
                    // In qua TCP/IP
                    if (!RawPrinterHelper.SendBytesToTcpPrinter(printerIp, printerPort, ms.ToArray()))
                    {
                        throw new Exception("Gửi dữ liệu đến máy in TCP/IP thất bại");
                    }
                }
                else
                {
                    // Không có IP => In qua USB
                    string printerName = formItems.FirstOrDefault()?.PrinterName;
                    if (!RawPrinterHelper.SendBytesToPrinter(printerName!, ms.ToArray()))
                    {
                        throw new Exception("Gửi dữ liệu đến máy in USB thất bại");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error printing form: {ex.Message}");
            }
        }

        public static Bitmap CreateBitmapFromText(string text, Font font, bool isCenter)
        {
            int maxWidth = 576; // chiều rộng tối đa khổ giấy (80mm ~ 576px)
            int padding = 5;

            using (Bitmap tempBitmap = new Bitmap(1, 1))
            using (Graphics tempGraphics = Graphics.FromImage(tempBitmap))
            {
                // Tạo StringFormat để căn chỉnh text
                StringFormat sf = new StringFormat();
                sf.FormatFlags = StringFormatFlags.LineLimit;
                sf.Alignment = isCenter ? StringAlignment.Center : StringAlignment.Near;

                // Đo kích thước text khi vẽ trong khung maxWidth
                SizeF textSize = tempGraphics.MeasureString(text, font, maxWidth - padding * 2, sf);

                int width = maxWidth;
                int height = (int)Math.Ceiling(textSize.Height) + padding * 2;

                Bitmap finalBitmap = new Bitmap(width, height);
                using (Graphics finalGraphics = Graphics.FromImage(finalBitmap))
                {
                    finalGraphics.Clear(System.Drawing.Color.White);
                    finalGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

                    RectangleF rect = new RectangleF(padding, padding, maxWidth - padding * 2, height - padding * 2);

                    // Vẽ text trong khung, tự động xuống dòng
                    finalGraphics.DrawString(text, font, Brushes.Black, rect, sf);
                    finalGraphics.Flush();
                }

                return finalBitmap;
            }
        }
        /// <summary>
        /// Chuyển đổi một ảnh Bitmap thành chuỗi byte theo chuẩn ESC/POS để máy in có thể đọc được
        /// </summary>
        /// <param name="bitmap">Ảnh cần chuyển đổi</param>
        /// <returns>Mảng byte chứa lệnh và dữ liệu ảnh</returns>
        public static byte[] ConvertBitmapToPrinterBytes(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Lệnh ESC/POS để in ảnh raster: GS v 0
                int widthInBytes = (bitmap.Width + 7) / 8;
                byte[] command = { 0x1D, 0x76, 0x30, 0, (byte)widthInBytes, (byte)(widthInBytes >> 8), (byte)bitmap.Height, (byte)(bitmap.Height >> 8) };
                ms.Write(command, 0, command.Length);

                // Vòng lặp để đọc từng pixel của ảnh và chuyển thành dữ liệu 1-bit (đen/trắng)
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int xByte = 0; xByte < widthInBytes; xByte++)
                    {
                        byte slice = 0;
                        for (int bit = 0; bit < 8; bit++)
                        {
                            int x = xByte * 8 + bit;
                            if (x < bitmap.Width)
                            {
                                // Lấy màu của pixel và kiểm tra độ sáng
                                // Nếu pixel không phải màu trắng (độ sáng < 0.5) thì coi là điểm đen
                                if (bitmap.GetPixel(x, y).GetBrightness() < 0.5f)
                                {
                                    slice |= (byte)(1 << (7 - bit));
                                }
                            }
                        }
                        ms.WriteByte(slice);
                    }
                }
                return ms.ToArray();
            }
        }
    }
}
