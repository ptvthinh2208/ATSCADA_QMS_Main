using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using Microsoft.AspNetCore.Components.Forms;

namespace ATSCADA_Client.Pages.DashBoard
{
    public partial class SettingList
    {
        private SettingUpdateRequest settingUpdateRequest = new SettingUpdateRequest();
        private List<WorkShift> workShifts = new List<WorkShift>();
        private IBrowserFile selectedVideoFile;

        private bool isAppointmentEnabled;
        private bool isSpeechCallEnabled;
        private TimeSpan? scheduledTaskTime;
        private DateTime? lastTaskExecuted;
        private bool isModalScheduledVisible = false;
        private bool isModalViewUIVisible = false;
        private bool isModalSettingWorkShift = false;
        private bool isConfirmModal = false;
        private string? footerTextCountersMainView { get; set; }
        private string? footerTextColor { get; set; }
        private string? footerTextFontSize { get; set; }
        private string? UrlVideoCountersMainView { get; set; }
        private string? videoUrlUpload { get; set; }
        //private string? videoUrlFromDB { get; set; }
        private string? MaxVisibleCounters { get; set; }
        //private string S
        protected override async Task OnInitializedAsync()
        {
            var dataSetting = await GetValueFromDatabase();
            int valueZNS = dataSetting.IsActiveSendZNS ? 1 : 0;
            int valueAppointment = dataSetting.IsAppointmentForm ? 1 : 0;
            int valueSpeechCall = dataSetting.IsActiveSpeechCall ? 1 : 0;

            isAppointmentEnabled = valueAppointment == 1;
            isSpeechCallEnabled = valueSpeechCall == 1;
            scheduledTaskTime = dataSetting.ScheduledTaskTime;
            lastTaskExecuted = dataSetting.LastTaskExecuted;
            workShifts = await GetListWorkShift();
            // Cập nhật các giá trị string để hiển thị trên form
            ScheduledTaskTimeString = scheduledTaskTime?.ToString(@"hh\:mm")!;
            LastTaskExecutedString = lastTaskExecuted?.ToString("dd/MM/yyyy hh:mm")!;
            footerTextCountersMainView = dataSetting.FooterTextCountersMainView;
            footerTextFontSize = dataSetting.FooterTextFontSize.ToString();
            footerTextColor = dataSetting.FooterTextColor;
            UrlVideoCountersMainView = dataSetting.UrlVideoCountersMainView;
            MaxVisibleCounters = dataSetting.MaxVisibleCounters.ToString();
        }
        private async Task<Setting> GetValueFromDatabase()
        {
            return await SettingApiClient.GetAllSettings();
        }
        private async Task<List<WorkShift>> GetListWorkShift()
        {
            return await WorkShiftApiClient.GetAllWorkShifts();
        }
        private bool IsAppointmentEnabled
        {
            get => isAppointmentEnabled;
            set
            {
                if (isAppointmentEnabled != value)
                {
                    isAppointmentEnabled = value;
                    SaveAppointmentToDatabase(isAppointmentEnabled ? 1 : 0);
                }
            }
        }
        private bool IsSpeechCallEnabled
        {
            get => isSpeechCallEnabled;
            set
            {
                if (isSpeechCallEnabled != value)
                {
                    isSpeechCallEnabled = value;
                    SaveSpeechCallStatusToDatabase(isSpeechCallEnabled ? 1 : 0);
                }
            }
        }
        // Các thuộc tính string để bind với InputText
        private string ScheduledTaskTimeString
        {
            get => scheduledTaskTime?.ToString(@"hh\:mm")!; // Định dạng giờ:phút
            set
            {
                if (TimeSpan.TryParse(value, out var result))
                {
                    scheduledTaskTime = result;
                }
            }
        }

        private string LastTaskExecutedString
        {
            get => lastTaskExecuted?.ToString("dd/MM/yyyy hh:mm")!; // Định dạng ngày/tháng/năm giờ:phút
            set
            {
                if (DateTime.TryParse(value, out var result))
                {
                    lastTaskExecuted = result;
                }
            }
        }


        private Task SaveAppointmentToDatabase(int status)
        {
            settingUpdateRequest.IsAppointmentForm = (status == 1);
            SettingApiClient.UpdateSettingAsync(1, settingUpdateRequest);
            return Task.CompletedTask;
        }
        private Task SaveSpeechCallStatusToDatabase(int status)
        {
            settingUpdateRequest.IsSpeechCall = (status == 1);
            SettingApiClient.UpdateSettingAsync(1, settingUpdateRequest);
            return Task.CompletedTask;
        }
        private void ConfirmExcuteTask()
        {
            isConfirmModal = true;
        }
        private async Task ExecuteNow()
        {
            isConfirmModal = true;
            var result = await SettingApiClient.RunScheduledTaskNow();
            if (result)
            {
                CloseModal();
                ToastService.ShowSuccess("Reset Successfully");
                StateHasChanged();
            }
            else ToastService.ShowError("Have Error!");
        }
        private void OpenSettingWorkShift()
        {
            isModalSettingWorkShift = true;
        }
        private void OpenEditModal(string nameModal)
        {

            if (nameModal == "ScheduledTaskTime")
            {
                isModalScheduledVisible = true; // Show the modal
            }
            if (nameModal == "FooterTextCountersMainView")
            {
                isModalViewUIVisible = true; // Show the modal
            }
        }
        private void CloseModal()
        {
            isConfirmModal = false;
            isModalScheduledVisible = false;
            isModalViewUIVisible = false;
            isModalSettingWorkShift = false;
            StateHasChanged(); // Cập nhật lại giao diện khi đóng modal
        }
        // Hàm lưu dữ liệu
        private async Task SaveSettings()
        {

            var settingUpdateRequest = new SettingUpdateRequest
            {
                ScheduledTaskTime = scheduledTaskTime,
                FooterTextCountersMainView = footerTextCountersMainView,
                VideoUrl = videoUrlUpload,
                FooterTextColor = footerTextColor,
                FooterTextFontSize = Int32.Parse(footerTextFontSize!),
                MaxVisibleCounters = Int32.Parse(MaxVisibleCounters!),

            };

            await SettingApiClient.UpdateSettingAsync(1, settingUpdateRequest);
            CloseModal();
            ToastService.ShowSuccess("Updated Successfully");
            StateHasChanged();

        }

        private async Task HandleSelectedVideo(InputFileChangeEventArgs e)
        {
            selectedVideoFile = e.File;

            if (selectedVideoFile != null)
            {
                try
                {
                    // Call service to upload the file
                    videoUrlUpload = await FileUploadApiClient.UploadFile(selectedVideoFile);
                    Console.WriteLine("File uploaded successfully: " + videoUrlUpload);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error uploading file: " + ex.Message);
                }
            }

        }
    }
}
