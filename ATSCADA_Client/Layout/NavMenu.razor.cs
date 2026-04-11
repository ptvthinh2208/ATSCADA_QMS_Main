using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Net.NetworkInformation;
using System.Security.Claims;

namespace ATSCADA_Client.Layout
{
    public partial class NavMenu
    {
        private bool collapseNavMenu = true;

        private string? NavMenuCssClass => collapseNavMenu ? "collapse" : "show"; // Thêm lớp 'show' khi mở
        private string CounterByServiceIdUrl => $"countercontrol/{CounterId}";
        private string ReportByServiceIdUrl => $"report/{CounterId}";
        private string LoutOutByUserNameUrl => $"logout/{UserNameFromJWT}";
        [Parameter]
        public int CounterId { set; get; }
        private string? UserNameFromJWT {  set; get; }
        private void ToggleNavMenu()
        {
            collapseNavMenu = !collapseNavMenu;
        }
        protected override async Task OnInitializedAsync()
        {
            CounterId = await GetServiceIdAsync();
            UserNameFromJWT = await GetUserNameFromJWT();
        }
        public async Task<int> GetServiceIdAsync()
        {
            var token = await JSRuntime.InvokeAsync<string>("getLocalStorage", "authToken");

            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var counterId = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
                Console.WriteLine(Int32.Parse(counterId));
                return Int32.Parse(counterId);
            }

            return 0;
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

            return "0";
        }
    }
}
