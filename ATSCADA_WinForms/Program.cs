using System;
using System.IO;
using System.Windows.Forms;
using ATSCADA_WinForms.Services;
using Newtonsoft.Json.Linq;

namespace ATSCADA_WinForms
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Đọc cấu hình từ file appsettings.json
            string apiBaseUrl = LoadApiBaseUrl(); 
            
            var apiService = new ApiService(apiBaseUrl);
            var signalRService = new SignalRService(apiBaseUrl + "queueHub");

            // Show Login First
            using (var loginForm = new LoginForm(apiService))
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    // If login successful, join the Main Dashboard
                    Application.Run(new MainForm(apiService, signalRService));
                }
            }
        }

        private static string LoadApiBaseUrl()
        {
            string defaultUrl = "https://localhost:44395/";
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

            try
            {
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    var config = JObject.Parse(json);
                    return config["ApiBaseUrl"]?.ToString() ?? defaultUrl;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đọc file cấu hình: " + ex.Message, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return defaultUrl;
        }
    }
}