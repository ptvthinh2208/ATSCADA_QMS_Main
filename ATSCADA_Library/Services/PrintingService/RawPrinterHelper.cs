using System.Drawing.Printing;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ATSCADA_Library.Services.PrintingService
{
    public class RawPrinterHelper
    {
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true)]
        public static extern bool OpenPrinter(string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter")]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, int level, ref DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter")]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter")]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter")]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter")]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        [StructLayout(LayoutKind.Sequential)]
        public struct DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }
        public static bool SendBytesToTcpPrinter(string printerIp, int port, byte[] data)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    // Kết nối đến máy in
                    client.Connect(printerIp, port);

                    // Gửi dữ liệu
                    using (NetworkStream stream = client.GetStream())
                    {
                        stream.Write(data, 0, data.Length);
                        stream.Flush();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending to TCP printer: {ex.Message}");
                return false;
            }
        }

        public static bool SendBytesToPrinter(string printerName, byte[] bytes)
        {
            bool success = false;
            IntPtr pUnmanagedBytes = IntPtr.Zero;
            IntPtr hPrinter = IntPtr.Zero;
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrinterErrorLog.txt");

            try
            {
                // ✅ Kiểm tra máy in tồn tại
                bool printerExists = false;
                foreach (string installedPrinter in PrinterSettings.InstalledPrinters)
                {
                    if (string.Equals(installedPrinter, printerName, StringComparison.OrdinalIgnoreCase))
                    {
                        printerExists = true;
                        break;
                    }
                }

                if (!printerExists)
                {
                    throw new Exception($"Không tìm thấy máy in: {printerName}");
                }

                // Gửi dữ liệu
                pUnmanagedBytes = Marshal.AllocCoTaskMem(bytes.Length);
                Marshal.Copy(bytes, 0, pUnmanagedBytes, bytes.Length);

                DOCINFOA di = new DOCINFOA
                {
                    pDocName = "ESC/POS Document",
                    pDataType = "RAW"
                };

                if (!OpenPrinter(printerName.Normalize(), out hPrinter, IntPtr.Zero))
                {
                    throw new Exception($"Không thể mở máy in: {printerName}");
                }

                if (!StartDocPrinter(hPrinter, 1, ref di))
                {
                    throw new Exception("Không thể bắt đầu tài liệu in");
                }

                if (!StartPagePrinter(hPrinter))
                {
                    throw new Exception("Không thể bắt đầu trang in");
                }

                if (!WritePrinter(hPrinter, pUnmanagedBytes, bytes.Length, out int bytesWritten) || bytesWritten != bytes.Length)
                {
                    throw new Exception("Lỗi khi ghi dữ liệu đến máy in");
                }

                EndPagePrinter(hPrinter);
                EndDocPrinter(hPrinter);
                success = true;
            }
            catch (Exception ex)
            {
                string errorText = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Lỗi: {ex.Message}\r\n";
                Console.WriteLine(errorText);
                File.AppendAllText(logPath, errorText);
            }
            finally
            {
                if (hPrinter != IntPtr.Zero)
                    ClosePrinter(hPrinter);

                if (pUnmanagedBytes != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(pUnmanagedBytes);
            }

            return success;
        }

    }
}
