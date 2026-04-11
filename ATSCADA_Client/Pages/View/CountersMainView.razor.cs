using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Library.Entities;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace ATSCADA_Client.Pages.View
{
    public partial class CountersMainView
    {
        private Queue currentClient = new Queue();
        private Setting dataSetting = new Setting();
        private List<Counter>? ListCounter = new List<Counter>();
        private List<Queue>? ListQueueWaitingRegister = new List<Queue>();
        private List<Queue>? ListQueueMissed = new List<Queue>();
        private List<Queue> DisplayedWaitingClients { get; set; } = new();
        private List<Queue> DisplayedMissedClients { get; set; } = new();
        private List<Queue> ListAllQueue = new List<Queue>();
        private List<QueueSpeech> listQueueSpeech = new List<QueueSpeech>();
        private PagingParameters paging = new PagingParameters();
        private Dictionary<int, List<Queue>> QueueDataByItemId = new Dictionary<int, List<Queue>>();
        private static SemaphoreSlim _speechSemaphore = new SemaphoreSlim(1, 1);
        private string? footerText { get; set; }
        private string footerTextColor { get; set; } = "#000000";
        private int footerFontSize { get; set; } = 16;
        private int MaxVisibleCounters { get; set; }
        private string? urlVideo { get; set; }
        public MetaData MetaData { get; set; } = new MetaData();
        private bool _isLoading = false;
        private int waitingStartIndex = 0;
        private int missedStartIndex = 0;
        private const int DisplayCount = 4;
        private System.Timers.Timer? rotationTimer;
        private static SemaphoreSlim _dataUpdateSemaphore = new SemaphoreSlim(1, 1);
        private bool _isDataLoading = false;
        private int waitingPageIndex = 0;
        private int missedPageIndex = 0;
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
                        //Console.WriteLine("SignalR connection started.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error starting SignalR connection: {ex.Message}");
                    }
                }

                await GetAllSetting();
                await GetCounterList();
                await GetListAllQueue();
                GetQueueMissed();
                await GetQueueSpeechList();
                await GetClientOnQueue();

                rotationTimer = new System.Timers.Timer(5000); // 10 giây
                rotationTimer.Elapsed += async (sender, e) => await RotateDisplay();
                rotationTimer.AutoReset = true;
                rotationTimer.Start();

                UpdateDisplayedLists();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnInitializedAsync: {ex.Message}");
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadVideoUrlAsync();
            }

            //if (urlVideo != null)
            //{
            //    await JSRuntime.InvokeVoidAsync("playVideo", "myVideo");
            //}
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
                _isDataLoading = true;
                rotationTimer?.Stop();

                await GetAllSetting();
                await GetCounterList();
                await GetListAllQueue();
                GetQueueMissed();
                await GetQueueSpeechList();
                await GetClientOnQueue();
                SpeechTextCallNumber();

                waitingStartIndex = 0;
                missedStartIndex = 0;
                waitingPageIndex = 0;
                missedPageIndex = 0;
                UpdateDisplayedLists();
                await InvokeAsync(StateHasChanged);

                rotationTimer?.Start();
            }
            finally
            {
                _isDataLoading = false;
                _dataUpdateSemaphore.Release();
            }
        }

        private async Task RotateDisplay()
        {
            //Console.WriteLine($"RotateDisplay triggered at: {DateTime.Now}");

            if (ListQueueWaitingRegister != null && ListQueueWaitingRegister.Any())
            {
                int totalItems = ListQueueWaitingRegister.Count;
                int totalPages = (int)Math.Ceiling((double)totalItems / DisplayCount);

                waitingPageIndex = (waitingPageIndex + 1) % totalPages;
                waitingStartIndex = waitingPageIndex * DisplayCount;
            }
            if (ListQueueMissed != null && ListQueueMissed.Any())
            {
                int totalItems = ListQueueMissed.Count;
                int totalPages = (int)Math.Ceiling((double)totalItems / DisplayCount);

                missedPageIndex = (missedPageIndex + 1) % totalPages;
                missedStartIndex = missedPageIndex * DisplayCount;
            }

            UpdateDisplayedLists();
            await InvokeAsync(StateHasChanged);
        }


        private void UpdateDisplayedLists()
        {
            //Console.WriteLine($"Updating displayed lists at: {DateTime.Now}");
            DisplayedWaitingClients.Clear();
            DisplayedMissedClients.Clear();

            // Waiting list
            if (ListQueueWaitingRegister != null && ListQueueWaitingRegister.Any())
            {
                DisplayedWaitingClients = ListQueueWaitingRegister
                    .Skip(waitingStartIndex)
                    .Take(DisplayCount)
                    .ToList();

                // Bổ sung null cho đủ 5 hàng nếu thiếu
                while (DisplayedWaitingClients.Count < DisplayCount)
                {
                    DisplayedWaitingClients.Add(null!);
                }


            }
            else
            {
                // Nếu không có dữ liệu, hiển thị toàn bộ là null
                DisplayedWaitingClients = Enumerable.Repeat<Queue>(null!, DisplayCount).ToList();
            }

            // Missed list
            if (ListQueueMissed != null && ListQueueMissed.Any())
            {
                DisplayedMissedClients = ListQueueMissed
                    .Skip(missedStartIndex)
                    .Take(DisplayCount)
                    .ToList();
                // Bổ sung null để đủ 3 hàng nếu thiếu
                while (DisplayedMissedClients.Count < DisplayCount)
                {
                    DisplayedMissedClients.Add(null!);
                }


            }
            else
            {
                DisplayedMissedClients = Enumerable.Repeat<Queue>(null!, DisplayCount).ToList();
            }
        }



        private async Task GetClientOnQueue()
        {
            ListQueueWaitingRegister.Clear();
            foreach (var item in ListCounter!.Take(MaxVisibleCounters))
            {
                QueueDataByItemId[item.Id] = await GetQueueWaitingAndRegister(item.Id);
            }
        }

        private async Task GetAllSetting()
        {
            dataSetting = await SettingApiClient.GetAllSettings();
            footerText = dataSetting.FooterTextCountersMainView;
            MaxVisibleCounters = dataSetting.MaxVisibleCounters;
            footerTextColor = dataSetting.FooterTextColor!;
            footerFontSize = dataSetting.FooterTextFontSize;
        }

        private async Task LoadVideoUrlAsync()
        {
            dataSetting = await SettingApiClient.GetAllSettings();
            urlVideo = dataSetting.UrlVideoCountersMainView!;
            //Console.WriteLine($"Loaded Video URL: {urlVideo}");
            StateHasChanged();
        }

        private async Task GetCounterList()
        {
            var listPagingResponse = await CounterApiClient.GetAllCounter(paging);
            ListCounter = listPagingResponse!.Items.Where(x => x.IsActive != false).ToList();
            MetaData = listPagingResponse.MetaData;
            StateHasChanged();
        }

        public async Task GetListAllQueue()
        {
            ListAllQueue = await QueueApiClient.GetListAllQueue();
            //Console.WriteLine($"Total Queues: {ListAllQueue?.Count}");
        }

        public async Task<List<Queue>> GetQueueWaitingAndRegister(int counterId)
        {
            //var listQueue = await QueueApiClient.GetQueuesByCounterIdAsync(counterId);
            ListQueueWaitingRegister = ListAllQueue
                .Where(x => x.Status == "Waiting")
                .OrderBy(x => x.OrderNumber)
                .ToList();

            UpdateDisplayedLists();
            return ListQueueWaitingRegister ?? new List<Queue>();
        }

        public void GetQueueMissed()
        {
            ListQueueMissed = ListAllQueue
                .Where(x => x.Status == "Missed" && x.LastTimeUpdated != null)
                .OrderBy(x => x.OrderNumber)
                .ToList();

        }
        public async Task<Service> GetServiceByServiceID(long ServiceId)
        {
            var code = await QueueApiClient.GetServiceById(ServiceId);
            return code;
        }

        public async Task GetQueueSpeechList()
        {
            var listFromDB = await QueueSpeechApiClient.GetQueueSpeechList();
            listQueueSpeech = listFromDB.Where(x => x.IsCompleted == false).ToList();
        }

        public async void SpeechTextCallNumber()
        {
            await _speechSemaphore.WaitAsync();
            try
            {
                if (listQueueSpeech != null && listQueueSpeech.Any() && dataSetting.IsActiveSpeechCall == true)
                {
                    for (int currentIndex = 0; currentIndex < listQueueSpeech.Count; currentIndex++)
                    {
                        var audioFile = await QueueSpeechApiClient.ConvertStringToBase64(listQueueSpeech[currentIndex].TextToSpeech!);
                        if (audioFile != null)
                        {
                            await QueueSpeechApiClient.UpdateQueueSpeechById(listQueueSpeech[currentIndex]);
                            string introUrl = "assets/audio/NHAC.mp3";
                            await JSRuntime.InvokeVoidAsync("playBase64AudioWithIntro", introUrl, audioFile);
                        }
                        else
                        {
                            Console.WriteLine("Failed to generate audio");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Queue is empty or speech call is inactive");
                }
            }
            finally
            {
                _speechSemaphore.Release();
            }
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
