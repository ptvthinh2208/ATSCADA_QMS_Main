using ATSCADA_API.Hubs;
using ATSCADA_API.Interfaces;
using ATSCADA_API.Repositories;
using ATSCADA_Library.Data;
using ATSCADA_Library.Helpers;
using ATSCADA_Library.Interfaces.Server;
using ATSCADA_Library.Mappers;
using ATSCADA_Library.Repositories;
using ATSCADA_Library.Services.ApiService;
using ATSCADA_Library.Services.ApiService.Interfaces;
using ATSCADA_Library.Services.ApiService.Services;
using ATSCADA_Library.Services.BackgroundServices;
using ATSCADA_Library.Services.PrintingService;
using GemBox.Spreadsheet;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Text.Json.Serialization.Metadata;

var builder = WebApplication.CreateBuilder(args);
// Fix lỗi Swagger không serialize được do Reflection bị tắt
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, new DefaultJsonTypeInfoResolver());
});
// Add services to the container.
builder.Services.AddControllers(); // For Web API controllers
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Entity Framework and MySQL
builder.Services.AddDbContext<ATSCADADbContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(5, 6, 16)));
});
// Bật nén Brotli
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true; // Chỉ bật khi chạy HTTPS
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
});
//Services Hub
builder.Services.AddSignalR();
builder.Services.Configure<JWTSection>(builder.Configuration.GetSection("JwtSection"));
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
// Register HttpClient for making HTTP requests
builder.Services.AddHttpClient();

// Register background service and custom services
builder.Services.AddScoped(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var facePath = Path.Combine(env.WebRootPath, "face_recognition");
    return new FaceRecognitionService(facePath);
});
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<CounterServiceBackground>();
builder.Services.AddScoped<ScheduledTaskService>();
builder.Services.AddScoped<QueueResetService>();
builder.Services.AddScoped<DailyReportService>();
builder.Services.AddScoped<GetDataReportService>();
builder.Services.AddScoped<ZaloApiService>();
builder.Services.AddHttpClient<TokenApiService>();
builder.Services.AddScoped<CounterService>();
builder.Services.AddScoped<IModbusService, ModbusService>();
builder.Services.AddHttpClient<ModbusService>();
builder.Services.AddHostedService<ClearQueueService>(); 
builder.Services.AddHostedService<QueueSpeechCleanupService>(); //Clear queue speech
builder.Services.AddHostedService<ZNSZaloBackgroundService>();
// builder.Services.AddHostedService<LedSyncService>();
// Đăng ký LedSyncService để inject vào Controller
// builder.Services.AddSingleton<LedSyncService>();
builder.Services.AddSingleton<TicketPrinterService>();
//DI Container

//builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<ISettingRepository, SettingRepository>();
builder.Services.AddScoped<IQueueRepository, QueueRepository>();
builder.Services.AddScoped<ICounterRepository, CounterRepository>();
builder.Services.AddScoped<IUserAccount, UserAccountRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<ISystemRoleRepository, SystemRoleRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IQueueSpeech, QueueSpeechRepository>();
builder.Services.AddScoped<IQueueHistoryRepository, QueueHistoryRepository>();
builder.Services.AddScoped<IZnsConfigRepository, ZnsConfigRepository>();
builder.Services.AddScoped<IWorkShiftRepository, WorkShiftRepository>();
builder.Services.AddScoped<IModbusRepository, ModbusRepository>();
//Service

//Setup CORS để trang booking có thể sử dụng API và nhận SingalR
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .WithOrigins(corsOrigins!)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
            
});
// Thêm dịch vụ cho Razor Pages và Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");


var app = builder.Build();

app.UseResponseCompression(); //


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

//app.UseResponseCompression();
app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();
app.UseStaticFiles();
// Serve the Blazor WebAssembly app
app.UseBlazorFrameworkFiles();
app.MapDefaultControllerRoute();


app.UseCors("AllowAll"); // Apply CORS policy
app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization();  // Enable authorization middleware

app.UseRouting();
// Map API controllers and SignalR hubs directly
app.MapControllers();
app.MapHub<QueueHub>("/queueHub");
app.MapHub<LedHub>("/ledHub");

app.MapFallbackToFile("index.html");


// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ATSCADADbContext>();
    await ATSCADADbContextSeed.SeedAsync(context);
}

app.Run();