using ATSCADA_API.Interfaces;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Interfaces.Server;
using ATSCADA_Library.Services.ApiService.Interfaces;
using AutoMapper;
using EasyModbus;
using System.Collections.Concurrent;
using System.Threading;

namespace ATSCADA_Library.Services.ApiService.Services
{
    public class ModbusService : IModbusService
    {
        private readonly IModbusRepository _modbusRepository;
        private readonly IMapper _mapper;

        public ModbusService(
            IModbusRepository modbusRepository,
            IMapper mapper)
        {
            _modbusRepository = modbusRepository;
            _mapper = mapper;
        }

        // ================= CRUD =================

        public async Task<ModbusDto?> GetByIdAsync(int id)
        {
            var entity = await _modbusRepository.GetByIdAsync(id);
            return _mapper.Map<ModbusDto>(entity);
        }

        public async Task<IEnumerable<ModbusDto>> GetAllAsync()
        {
            var entities = await _modbusRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ModbusDto>>(entities);
        }

        public async Task AddAsync(ModbusDto dto)
        {
            var entity = _mapper.Map<Modbus>(dto);
            await _modbusRepository.AddAsync(entity);
            dto.Id = entity.Id;
        }

        public async Task UpdateAsync(int id, ModbusDto dto)
        {
            if (id != dto.Id) throw new ArgumentException("ID mismatch");

            var entity = await _modbusRepository.GetByIdAsync(id);
            if (entity == null) throw new KeyNotFoundException("Modbus config not found");

            _mapper.Map(dto, entity);
            await _modbusRepository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _modbusRepository.DeleteAsync(id);
        }

        // ================= SESSION MANAGER =================

        private class ModbusSession
        {
            public ModbusClient Client { get; set; }
            public SemaphoreSlim Lock { get; } = new SemaphoreSlim(1, 1);
        }

        private static readonly ConcurrentDictionary<int, ModbusSession> _sessions = new();
        private static readonly ConcurrentDictionary<int, ushort> _lastSentValue = new();

        // ================= PUBLIC SEND =================

        public async Task TransmitOrderNumberAsync(int modbusId, ushort newValue)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            await InternalTransmit(modbusId, newValue, cts.Token);
        }

        // ================= CORE =================

        private async Task InternalTransmit(int modbusId, ushort newValue, CancellationToken ct)
        {
            var session = await GetOrCreateSession(modbusId);

            await session.Lock.WaitAsync(ct);
            try
            {
                if (!session.Client.Connected)
                    session.Client.Connect();

                // ===== CACHE CHECK =====
                if (_lastSentValue.TryGetValue(modbusId, out ushort oldValue)
                    && oldValue == newValue)
                    return;

                // ===== WRITE MODBUS =====
                session.Client.WriteSingleRegister(0, newValue);

                // Save cache
                _lastSentValue[modbusId] = newValue;
            }
            catch
            {
                _lastSentValue.TryRemove(modbusId, out _);

                try { if (session.Client.Connected) session.Client.Disconnect(); } catch { }

                _sessions.TryRemove(modbusId, out _);
                throw;
            }
            finally
            {
                session.Lock.Release();
            }
        }

        // ================= SESSION FACTORY =================

        private async Task<ModbusSession> GetOrCreateSession(int modbusId)
        {
            if (_sessions.TryGetValue(modbusId, out var existing))
                return existing;

            var cfg = await _modbusRepository.GetByIdAsync(modbusId);
            if (cfg == null)
                throw new Exception($"Không tìm thấy Modbus config ID {modbusId}");

            var client = new ModbusClient(cfg.IpAddress, 502)
            {
                UnitIdentifier = (byte)cfg.SlaveId,
                ConnectionTimeout = 2000
            };

            client.Connect();

            var session = new ModbusSession { Client = client };
            _sessions[modbusId] = session;

            return session;
        }
    }
}
