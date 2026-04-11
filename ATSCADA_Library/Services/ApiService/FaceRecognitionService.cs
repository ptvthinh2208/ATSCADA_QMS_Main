using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ATSCADA_Library.Services.ApiService
{
    public class FaceRecognitionService
    {
        private readonly string _scriptsPath; // Thư mục chứa search_faces.exe, add_face.exe

        public FaceRecognitionService(string scriptsPath)
        {
            _scriptsPath = scriptsPath; // Ví dụ: "wwwroot/face_recognition"
        }

        public async Task<Dictionary<string, object>> SearchFaceAsync(string imagePath)
        {
            try
            {
                var result = await RunExeAsync("search_faces.exe", $"\"{imagePath}\"");
                return result;
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { { "error", $"Search failed: {ex.Message}" } };
            }
        }

        public async Task<Dictionary<string, object>> AddFaceAsync(string imagePath, string name, string info)
        {
            if (string.IsNullOrEmpty(name))
                return new Dictionary<string, object> { { "error", "Name is required" } };

            try
            {
                var result = await RunExeAsync("add_face.exe", $"\"{imagePath}\" \"{name}\" \"{info}\"");
                return result;
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { { "error", $"Add failed: {ex.Message}" } };
            }
        }

        private async Task<Dictionary<string, object>> RunExeAsync(string exeName, string arguments)
        {
            var exePath = Path.Combine(_scriptsPath, exeName);
            if (!File.Exists(exePath))
                throw new FileNotFoundException($"Executable not found: {exePath}");

            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8, 
                StandardErrorEncoding = Encoding.UTF8   //lỗi cũng in ra UTF-8
            };

            using (var process = new Process { StartInfo = psi })
            {
                process.Start();

                string output = "";
                string error = "";
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                if (!await Task.Run(() => process.WaitForExit(30000))) // Timeout 30s
                {
                    process.Kill();
                    throw new Exception($"Executable {exeName} timed out.");
                }

                output = await outputTask;
                error = await errorTask;

                if (!string.IsNullOrEmpty(error))
                    throw new Exception($"Executable error: {error}");

                // Parse JSON output
                try
                {
                    return JsonSerializer.Deserialize<Dictionary<string, object>>(output);
                }
                catch (Exception ex)
                {
                    throw new Exception($"JSON parse error: {ex.Message}");
                }
            }
        }
    }
}