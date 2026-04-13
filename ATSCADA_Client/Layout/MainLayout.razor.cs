using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ATSCADA_Client.Layout
{
    public partial class MainLayout
    {
        private bool isChangePasswordModalVisible = false;
        [Parameter]
        public string? UserName { set; get; }
        private ChangePasswordModel changePasswordModel = new ChangePasswordModel();
        private ApplicationUser currentUser = new ApplicationUser();
        protected override async Task OnInitializedAsync()
        {
            // Thực hiện logic đổi mật khẩu tại đây, ví dụ gọi API để xử lý đổi mật khẩu
            var userName = await GetUserNameFromJWT();
            if(userName.Any() && userName != "")
            {
                currentUser = await UserApiClient.GetUserByUserName(userName);
            }
        }
        public async Task<string> GetUserNameFromJWT()
        {
            var token = await JSRuntime.InvokeAsync<string>("getLocalStorage", "authToken");

            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var userName = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;
                return userName;
            }

            return "";
        }
        // Mở modal
        private void OpenChangePasswordModal()
        {
            isChangePasswordModalVisible = true;
        }
        // Đóng modal
        private void CloseChangePasswordModal()
        {
            isChangePasswordModalVisible = false;
        }
        // Xử lý việc đổi mật khẩu
        private async Task HandleChangePassword()
        {

            var updateResult = await UserApiClient.UpdatePasswordForChange(changePasswordModel, currentUser);
            // Kiểm tra kết quả cập nhật từ API
            if (updateResult)
            {
                ToastService.ShowSuccess("Password update successfull");
            }
            else
            {
                ToastService.ShowError("Old password is invalid");
            }


            // Đóng modal thay đổi mật khẩu
            isChangePasswordModalVisible = false;
        }
    }
}
