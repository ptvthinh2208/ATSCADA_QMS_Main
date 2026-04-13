using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Interfaces.Server;
using DocumentFormat.OpenXml.Wordprocessing;
using Humanizer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

public class ZaloApiService
{
    private readonly HttpClient _httpClient;
    private readonly IZnsConfigRepository _znsConfigRepository;

    public ZaloApiService(HttpClient httpClient, IZnsConfigRepository znsConfigRepository)
    {
        _httpClient = httpClient;
        _znsConfigRepository = znsConfigRepository;
    }
    public async Task<string> SendTemplateMessageAsync(Queue clientOnQueue, Counter selectedCounter, string templateName)
    {

        TokenApiService _tokenApiService = new TokenApiService(_httpClient, _znsConfigRepository);
        // Địa chỉ URL API của Zalo
        var url = "https://business.openapi.zalo.me/message/template";
        var znsInfo = await _znsConfigRepository.GetZnsInfoAsync();

        //Ghép chuỗi Tên dịch vụ - Số thứ tự => ví dụ : A02-001
        var order_codeFormated = $"{selectedCounter.Code}-{clientOnQueue.OrderNumber:D3}";
        var appointmentTimeFormated = $"{clientOnQueue.AppointmentTime.ToString("HH:mm")} {clientOnQueue.AppointmentDate.ToString("dd/MM/yyyy")}";
        var templateInfo = znsInfo.FirstOrDefault(x => x.TemplateName == templateName);
        if (templateInfo != null)
        {
            var body = new
            {
                phone = "84" + clientOnQueue.PhoneNumber!.Substring(1),
                template_id = templateInfo.TemplateID,
                template_data = new
                {
                    order_code = order_codeFormated,
                    full_name = clientOnQueue.FullName,
                    phone_number = clientOnQueue.PhoneNumber,
                    date = appointmentTimeFormated,
                    name = clientOnQueue.FullName
                },
                tracking_id = "tracking_id"
            };

            // Chuyển đổi body sang định dạng JSON
            var jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // Thêm access_token vào header
            _httpClient.DefaultRequestHeaders.Add("access_token", znsInfo[0].AccessToken);


            // Gửi yêu cầu POST
            var response = await _httpClient.PostAsync(url, content);

            // Kiểm tra và trả về kết quả
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(responseContent))
                {
                    JsonElement root = doc.RootElement;
                    string message = root.GetProperty("message").GetString()!;
                    if (message == "Success")
                    {
                        //model.Status = "Waiting";
                        //await _queueRepository.UpdateQueueStatusAsync(model);
                        return responseContent;
                    }
                    else if (message == "Access token invalid")
                    {
                        var refreshToken = await _tokenApiService.RefreshAccessToken();
                        if (refreshToken != null)
                        {
                            await SendTemplateMessageAsync(clientOnQueue, selectedCounter, templateName);
                        }
                        else { return responseContent; }

                    }
                }
                return responseContent; // Trả về kết quả từ API
            }
            else
            {
                // Xử lý lỗi nếu có
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Request failed with status {response.StatusCode}: {errorContent}");
            }

        }
        return null!;


    }
    public async Task<bool> SendZNSNotificationAsync(Queue model, Counter selectedCounter)
    {
        TokenApiService _tokenApiService = new TokenApiService(_httpClient, _znsConfigRepository);

        //var client = _httpClientFactory.CreateClient();
        string url = "https://business.openapi.zalo.me/message/template";
        //var znsInfo = await _tokenService.LoadZNSInfoAsync();
        var znsInfo = await _znsConfigRepository.GetZnsInfoAsync();
        var bookingCode = $"{selectedCounter.Code}-{model.OrderNumber:D3}";
        //var templateInfo = znsInfo.FirstOrDefault(x => x.TemplateName == templateName);
        var data = new
        {
            phone = "84" + model.PhoneNumber!.Substring(1),
            template_id = "361455",
            template_data = new
            {
                booking_code = bookingCode,
                address = "60 Đường số 1, P.Tân Thành, Q.Tân Phú, TP.HCM",
                schedule_time = model.AppointmentTime.ToString("HH:mm dd/MM/yyyy"),
                customer_name = model.FullName
            },
            tracking_id = "tracking_id"
        };
        // Chuyển đổi body sang định dạng JSON
        var jsonBody = JsonConvert.SerializeObject(data);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        // Thêm access_token vào header
        _httpClient.DefaultRequestHeaders.Add("access_token", znsInfo[0].AccessToken);


        // Gửi yêu cầu POST
        var response = await _httpClient.PostAsync(url, content);
        // Kiểm tra và trả về kết quả
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            using (JsonDocument doc = JsonDocument.Parse(responseContent))
            {
                JsonElement root = doc.RootElement;
                string message = root.GetProperty("message").GetString()!;
                if (message == "Success")
                {
                    //model.Status = "Waiting";
                    //await _queueRepository.UpdateQueueStatusAsync(model);
                    return true;
                }
                else if (message == "Access token invalid")
                {
                    var refreshToken = await _tokenApiService.RefreshAccessToken();
                }
            }
            return false; // Trả về kết quả từ API
        }
        else
        {
            // Xử lý lỗi nếu có
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request failed with status {response.StatusCode}: {errorContent}");
        }
    }
    public async Task<string> SendTemplateMessageForAllAsync(List<QueueHistory> listModel, string templateName)
    {
        TokenApiService _tokenApiService = new TokenApiService(_httpClient, _znsConfigRepository);
        // Địa chỉ URL API của Zalo
        var url = "https://business.openapi.zalo.me/message/template";
        var znsInfo = await _znsConfigRepository.GetZnsInfoAsync();
        var templateInfo = znsInfo.FirstOrDefault(x => x.TemplateName == templateName);
        if (templateInfo != null)
        {
            // Thêm access_token vào header một lần trước khi bắt đầu vòng lặp
            _httpClient.DefaultRequestHeaders.Remove("access_token"); // Xóa nếu đã tồn tại trước đó
            _httpClient.DefaultRequestHeaders.Add("access_token", znsInfo[0].AccessToken);
            foreach (var customer in listModel)
            {
                var body = new
                {
                    phone = "84" + customer.PhoneNumber!.Substring(1),
                    template_id = templateInfo.TemplateID,
                    template_data = new
                    {
                        booking_code = customer.OrderNumber,
                        address = "60 Đường số 1, P.Tân Thành, Q.Tân Phú, TP.HCM",
                        schedule_time = customer.AppointmentDate.ToString("dd/MM/yyyy"),
                        customer_name = customer.FullName
                    },
                    tracking_id = "tracking_id"
                };
                // Chuyển đổi body sang định dạng JSON
                var jsonBody = JsonConvert.SerializeObject(body);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                //// Thêm access_token vào header
                //_httpClient.DefaultRequestHeaders.Add("access_token", znsInfo[0].AccessToken);

                // Gửi yêu cầu POST
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    // Nếu có lỗi xảy ra, lấy nội dung lỗi để xử lý hoặc ghi log
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error sending message to {customer.PhoneNumber}: {errorContent}");
                }
            }
        }
        return null!;
    }

}