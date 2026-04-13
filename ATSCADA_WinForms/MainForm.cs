using System;
using System.Drawing;
using System.Windows.Forms;
using ATSCADA_WinForms.Services;
using ATSCADA_Library.Entities;
using System.Threading.Tasks;

namespace ATSCADA_WinForms
{
    public partial class MainForm : Form
    {
        private ApiService _apiService;
        private SignalRService _signalRService;
        private Counter _currentCounter;
        private Queue _currentQueue;

        public MainForm(ApiService apiService, SignalRService signalRService)
        {
            _apiService = apiService;
            _signalRService = signalRService;
            InitializeComponent();
            try { this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
            SetupEvents();
            InitializeLogic();
        }

        private void SetupEvents()
        {
            this.KeyUp += MainForm_KeyUp;
            btnNext.Click += async (s, e) => await CallNext();
            btnPrevious.Click += async (s, e) => await CallPrevious();
            btnReset.Click += async (s, e) => await ResetNumber();
            btnCallAny.Click += (s, e) => ToggleCallAny(true);
            btnSendAny.Click += async (s, e) => await SendAny();
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F1: btnNext.PerformClick(); break;
                case Keys.F2: btnPrevious.PerformClick(); break;
                case Keys.F3: btnReset.PerformClick(); break;
                case Keys.F4: btnCallAny.PerformClick(); break;
                case Keys.Enter: if (pnlCallAny.Visible) btnSendAny.PerformClick(); break;
                case Keys.Escape: ToggleCallAny(false); break;
            }
        }

        private async void InitializeLogic()
        {
            await LoadData();
            _signalRService.OnMessageReceived += async () => {
                await LoadData();
            };
            await _signalRService.StartAsync();
        }

        private async Task LoadData()
        {
            try
            {
                _currentCounter = await _apiService.GetCounterAsync(_apiService.CounterId);
                if (_currentCounter != null)
                {
                    string modbusIp = "Unknown";
                    var modbus = await _apiService.GetModbusAsync(_currentCounter.ModbusId);
                    if (modbus != null) modbusIp = modbus.IpAddress;

                    this.Invoke((MethodInvoker)delegate {
                        lblCounterInfo.Text = $"Đã kết nối đến bảng LED IP: {modbusIp}";
                        lblCurrentNumber.Text = _currentCounter.CurrentNumber.ToString("D3");
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading data: " + ex.Message);
            }
        }

        private async Task CallNext()
        {
            if (_currentCounter == null) return;
            var queue = await _apiService.CallNextAsync(_apiService.CounterId, _currentCounter.ServiceID);
            if (queue != null)
            {
                _currentQueue = queue;
                // Transmit to LED via Modbus
                await _apiService.TransmitModbusAsync(_currentCounter.ModbusId, queue.OrderNumber);
                await LoadData();
            }
            else
            {
                MessageBox.Show("Không có khách hàng chờ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async Task CallPrevious()
        {
            if (_currentCounter == null) return;
            var queue = await _apiService.CallPreviousAsync(_apiService.CounterId, _currentCounter.ServiceID);
            if (queue != null)
            {
                _currentQueue = queue;
                // Transmit to LED via Modbus
                await _apiService.TransmitModbusAsync(_currentCounter.ModbusId, queue.OrderNumber);
                await LoadData();
            }
            else
            {
                MessageBox.Show("Không còn khách hàng phía trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async Task ResetNumber()
        {
            if (_currentCounter == null) return;
            if (MessageBox.Show("Bạn có chắc chắn muốn Reset số hiện tại về 0?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                await _apiService.UpdateCounterNumberAsync(_apiService.CounterId, 0);
                await _apiService.TransmitModbusAsync(_currentCounter.ModbusId, 0);
                await LoadData();
            }
        }

        private async Task Recall()
        {
            if (_currentCounter == null || _currentCounter.CurrentNumber == 0) return;
            // Get current queue to get detail like name etc if needed, but the simple recall uses number
            await _apiService.RecallAsync(_apiService.CounterId, _currentCounter.CurrentNumber, "", "", _currentCounter.Name, _currentCounter.Code);
            // Also refresh LED
            await _apiService.TransmitModbusAsync(_currentCounter.ModbusId, _currentCounter.CurrentNumber);
        }

        private async Task Skip()
        {
            if (_currentQueue == null)
            {
                MessageBox.Show("Cần gọi khách hàng trước khi bỏ qua.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            await _apiService.UpdateQueueStatusAsync(_currentQueue.Id, "Missed");
            await LoadData();
        }

        private void ToggleCallAny(bool show)
        {
            pnlCallAny.Visible = show;
            if (show) txtAnyNumber.Focus();
        }

        private async Task SendAny()
        {
            if (int.TryParse(txtAnyNumber.Text, out int num))
            {
                // Custom logic for "Call Any":
                // 1. Update counter's current number
                // 2. Transmit to LED via modbus
                await _apiService.UpdateCounterNumberAsync(_apiService.CounterId, num);
                await _apiService.TransmitModbusAsync(_currentCounter.ModbusId, num);
                
                ToggleCallAny(false);
                txtAnyNumber.Clear();
                await LoadData();
            }
            else
            {
                MessageBox.Show("Vui lòng nhập số hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
