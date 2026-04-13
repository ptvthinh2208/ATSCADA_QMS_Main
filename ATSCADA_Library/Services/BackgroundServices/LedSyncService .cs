using ATSCADA_Library.Entities;
using ATSCADA_Library.Interfaces.Server;
using ATSCADA_Library.Services.ApiService.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace ATSCADA_Library.Services.BackgroundServices
{
    public class LedSyncService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LedSyncService> _logger;
        private readonly Channel<LedUpdateRequest> _updateChannel;
        private readonly ConcurrentDictionary<int, ushort> _lastSentValues = new();

        public LedSyncService(IServiceScopeFactory scopeFactory, ILogger<LedSyncService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _updateChannel = Channel.CreateBounded<LedUpdateRequest>(new BoundedChannelOptions(100)
            {
                SingleWriter = false,
                SingleReader = true,
                FullMode = BoundedChannelFullMode.DropOldest
            });
        }

        // Phương thức public để nhận tín hiệu từ API (khi Call Next)
        public async Task TriggerLedUpdateAsync(int modbusId, ushort value)
        {
            await _updateChannel.Writer.WriteAsync(new LedUpdateRequest(modbusId, value));
            _logger.LogInformation($"[TRIGGER] Đã gửi lệnh LED: {value} tới Modbus {modbusId}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var pollTask = PollDatabaseForChangesAsync(stoppingToken);
            var transmitTask = ProcessModbusQueueAsync(stoppingToken);

            await Task.WhenAll(pollTask, transmitTask);
        }

        private async Task PollDatabaseForChangesAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var counterRepo = scope.ServiceProvider.GetRequiredService<ICounterRepository>();

                    // ⚠️ Nếu repo của bạn chưa hỗ trợ CancellationToken, bỏ tham số ct đi
                    var counters = await counterRepo.GetAllCountersRawAsync();

                    foreach (var counter in counters)
                    {
                        if (!ushort.TryParse(counter.Code + counter.CurrentNumber.ToString("D3"), out ushort ledValue))
                            continue;

                        // ✅ CHANGE DETECTION: Chỉ queue nếu giá trị THỰC SỰ thay đổi
                        if (_lastSentValues.TryGetValue(counter.ModbusId, out ushort last) && last == ledValue)
                            continue;

                        await _updateChannel.Writer.WriteAsync(new LedUpdateRequest(counter.ModbusId, ledValue), ct);
                        _lastSentValues[counter.ModbusId] = ledValue; // Cache local để so sánh lần sau
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[POLL] Lỗi khi load counters");
                }

                // ✅ Giảm tần suất xuống 2s (vì đã có cache so sánh, không cần poll 1s)
                await Task.Delay(2000, ct);
            }
        }

        private async Task ProcessModbusQueueAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    // Chờ có request mới trong queue
                    var request = await _updateChannel.Reader.ReadAsync(ct);

                    using var scope = _scopeFactory.CreateScope();
                    var modbusService = scope.ServiceProvider.GetRequiredService<IModbusService>();

                    try
                    {
                        // ✅ Gọi đúng signature cũ của bạn (không truyền CancellationToken)
                        await modbusService.TransmitOrderNumberAsync(request.ModbusId, request.Value);
                        _logger.LogDebug("[TX] Đã gửi {Value} tới Modbus {Id}", request.Value, request.ModbusId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[TX] Gửi thất bại {ModbusId}", request.ModbusId);
                        // Nếu lỗi mạng/Modbus, xóa cache để lần poll sau thử gửi lại
                        _lastSentValues.TryRemove(request.ModbusId, out _);
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[QUEUE] Lỗi xử lý queue");
                }
            }
        }

        private record LedUpdateRequest(int ModbusId, ushort Value);
    }
}
