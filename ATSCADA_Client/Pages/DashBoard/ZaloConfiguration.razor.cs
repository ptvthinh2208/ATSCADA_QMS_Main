using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;


namespace ATSCADA_Client.Pages.DashBoard
{
    public partial class ZaloConfiguration
    {
        private SettingUpdateRequest settingUpdateRequest = new SettingUpdateRequest();
        private List<ZnsInfoDto> znsInfos = new List<ZnsInfoDto>();
        private ZnsConfig znsConfig = new ZnsConfig();
        private bool isZaloEnabled;
        private string? TemplateName {  get; set; }
        private bool showModalSelectTemplate { get; set; }
        private bool showModalConfigZnsInfo { get; set; }
        private bool showModalListTemplate { get; set; }
        //private bool isEditing = false;

        protected override async Task OnInitializedAsync()
        {
            var dataSetting = await GetSettingFromDatabase();
            TemplateName = dataSetting.SendZnsWithTemplate;
            int valueZNS = dataSetting.IsActiveSendZNS ? 1 : 0;
            isZaloEnabled = valueZNS == 1;
            znsInfos = await GetZnsTemplate();
            znsConfig.AppID = znsInfos[0].AppID;
            znsConfig.SecretKey = znsInfos[0].SecretKey;
            znsConfig.AccessToken = znsInfos[0].AccessToken;
            znsConfig.RefreshToken = znsInfos[0].RefreshToken;
        }
        private async Task<Setting> GetSettingFromDatabase()
        {
            return await SettingApiClient.GetAllSettings();
        }
        private async Task<List<ZnsInfoDto>> GetZnsTemplate()
        {
            return await ZnsConfigApiClient.GetZnsInfoAsync();
        }
        
        private bool IsZaloEnabled
        {
            get => isZaloEnabled;
            set
            {
                if (isZaloEnabled != value)
                {
                    isZaloEnabled = value;
                    SaveZaloStatusToDatabase(isZaloEnabled ? 1 : 0);
                    // Kiểm tra trạng thái và hiển thị modal
                    if (isZaloEnabled)
                    {
                        showModalSelectTemplate = true;
                    }
                }
            }
        }
        
        private void CloseModal()
        {
            showModalSelectTemplate = false;
            showModalConfigZnsInfo = false;
            showModalListTemplate = false;
            StateHasChanged();
        }
        private Task SaveZaloStatusToDatabase(int status)
        {
            settingUpdateRequest.IsActiveSendZNS = (status == 1);
            SettingApiClient.UpdateSettingAsync(1, settingUpdateRequest);
            return Task.CompletedTask;
        }
        private async Task SaveZnsInfo()
        {
            if (znsConfig.AppID != null && znsConfig.SecretKey != null 
                && znsConfig.AccessToken != null && znsConfig.RefreshToken != null)
            {
                var dto = new ZnsInfoDto
                {
                    AppID = znsConfig.AppID,
                    SecretKey = znsConfig.SecretKey,
                    RefreshToken = znsConfig.RefreshToken,
                    AccessToken = znsConfig.AccessToken,
                };

                await ZnsConfigApiClient.UpdateZnsInfoAsync(dto);
                CloseModal();
                ToastService.ShowSuccess("Updated Successfully");
                StateHasChanged();
            }
            else
            {
                ToastService.ShowError("Please select your Template ZNS");
                IsZaloEnabled = false;
                StateHasChanged();
            }
        }
        // Hàm lưu dữ liệu
        private async Task SaveSettings()
        {
            if(TemplateName != null && TemplateName.Any())
            {
                var settingUpdateRequest = new SettingUpdateRequest
                {
                    ZnsTemplateName = TemplateName,
                    IsActiveSendZNS = true
                };

                await SettingApiClient.UpdateSettingAsync(1, settingUpdateRequest);
                CloseModal();
                ToastService.ShowSuccess("Updated Successfully");
                StateHasChanged();
            }
            else
            {
                ToastService.ShowError("Please select your Template ZNS");
                IsZaloEnabled = false;
                StateHasChanged();
            }
        }
        private void OpenModalDetailZnsInfo()
        {

            showModalConfigZnsInfo = true;
        }
        private void OpenModalListTemplate()
        {

            showModalListTemplate = true;
        }
    }
}
