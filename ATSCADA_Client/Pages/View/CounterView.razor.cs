using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Library.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace ATSCADA_Client.Pages.View
{
    public partial class CounterView
    {
        [Parameter]
        public int CounterId { set; get; }
        private Queue currentClient = new Queue();

        private Counter Counter = new Counter();
        public MetaData MetaData { get; set; } = new MetaData();
        private bool _isLoading = false;

        private System.Timers.Timer? rotationTimer;
        private static SemaphoreSlim _dataUpdateSemaphore = new SemaphoreSlim(1, 1);
        private bool _isDataLoading = false;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                if (HubConnection.State == HubConnectionState.Disconnected)
                {
                    try
                    {
                        HubConnection.On("ReceiveMessage", () =>
                        {
                            CallLoadData();
                        });
                        await HubConnection.StartAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error starting SignalR connection: {ex.Message}");
                    }
                }
                await GetCounterByIdAsync();

                rotationTimer = new System.Timers.Timer(5000); // 10 giây
                //rotationTimer.Elapsed += async (sender, e) => await RotateDisplay();
                rotationTimer.AutoReset = true;
                rotationTimer.Start();

               // UpdateDisplayedLists();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnInitializedAsync: {ex.Message}");
            }
        }

        private async Task HandleVideoClick()
        {
            await JSRuntime.InvokeVoidAsync("toggleVideoMuteOnClick", "myVideo");
        }

        private async void CallLoadData()
        {
            if (_isDataLoading) return;

            await _dataUpdateSemaphore.WaitAsync();
            try
            {
                await GetCounterByIdAsync();

                //UpdateDisplayedLists();
                await InvokeAsync(StateHasChanged);
            }
            finally
            {
                _isDataLoading = false;
                _dataUpdateSemaphore.Release();
            }
        }

        
        //Lấy danh sách khách hàng ở trong hàng đợi theo tên Counter
        private async Task GetCounterByIdAsync()
        {
            if (CounterId != 0)
            {
                Counter = await CounterApiClient.GetCounterById(CounterId);
                StateHasChanged();
            }
            else Console.WriteLine("Load client on queue failed");
        }

        public string GetTimeDelay(DateTime lastTimeUpdated)
        {
            TimeSpan timeDelay = DateTime.Now - lastTimeUpdated;
            if (timeDelay.TotalMinutes < 1)
                return $"{(int)timeDelay.TotalSeconds} secs ago";
            else if (timeDelay.TotalHours < 1)
                return $"{(int)timeDelay.TotalMinutes} mins ago";
            else if (timeDelay.TotalDays < 1)
                return $"{(int)timeDelay.TotalHours} hours ago";
            else
                return $"{(int)timeDelay.TotalDays} days ago";
        }

        public void Dispose()
        {
            rotationTimer?.Stop();
            rotationTimer?.Dispose();
            _dataUpdateSemaphore?.Dispose();
        }
    }
}
