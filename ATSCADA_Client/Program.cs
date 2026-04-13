using ATSCADA_Client;
using ATSCADA_Client.Services;
using ATSCADA_Client.Helpers;
using ATSCADA_Client.Interfaces.Client;
using ATSCADA_Client.Services.ApiClientService;
using ATSCADA_Client.Services.Authenticate;
using Blazored.LocalStorage;
using Blazored.Toast;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Services;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;




var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Logging.SetMinimumLevel(LogLevel.Warning); // Chỉ hiển thị từ Warning trở lên
// Hoặc lọc chi tiết các logger cụ thể
builder.Logging.AddFilter("Microsoft.AspNetCore.Authorization.DefaultAuthorizationService", LogLevel.None);
// Add configuration support
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();
builder.Services.AddBlazoredToast();
builder.Services.AddScoped<LazyAssemblyLoader>();
//Lấy url API từ file appsetting.json để kết nối với SingalR
var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl");
builder.Services.AddSingleton(sp => new HubConnectionBuilder()
    .WithUrl(new Uri($"{apiBaseUrl!}queueHub"))
    .WithAutomaticReconnect()
    .Build());

//Register DI Client
//builder.Services.AddScoped<IAppointmentApiClient, AppointmentApiClient>();
builder.Services.AddScoped<ISettingApiClient, SettingApiClient>();
builder.Services.AddScoped<IQueueApiClient, QueueApiClient>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICounterApiClient, CounterApiClient>();
builder.Services.AddScoped<IFeedbackApiClient, FeedbackApiClient>();
builder.Services.AddScoped<IExportApiClient, ExportApiClient>();
builder.Services.AddScoped<IUserApiClient, UserApiClient>();
builder.Services.AddScoped<ISystemRoleApiClient, SystemRoleApiClient>();
builder.Services.AddScoped<IReportApiClient, ReportApiClient>();
builder.Services.AddScoped<IQueueSpeechApiClient, QueueSpeechApiClient>();
builder.Services.AddScoped<IQueueHistoryApiClient, QueueHistoryApiClient>();
builder.Services.AddScoped<IFileUploadApiClient, FileUploadApiClient>();
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();

builder.Services.AddScoped<IZnsConfigApiClient, ZnsConfigApiClient>();
builder.Services.AddScoped<IWorkShiftApiClient, WorkShiftApiClient>();
builder.Services.AddSingleton<IConfiguration>(config);
// Existing code...

builder.Services.AddBlazoredLocalStorage(); // This line will now work correctly
builder.Services.AddAuthorizationCore();

// Configure HttpClient for API calls
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});



var host = builder.Build();
// Fetch the base path from API and set it in the <base> tag
var httpClient = host.Services.GetRequiredService<HttpClient>();
var basePath = await httpClient.GetStringAsync("api/configuration/basepath");

// Set the base href dynamically based on the base path
await host.Services.GetRequiredService<IJSRuntime>().InvokeVoidAsync("setBaseHref", string.IsNullOrEmpty(basePath) ? "/" : $"/{basePath}");


await builder.Build().RunAsync();
