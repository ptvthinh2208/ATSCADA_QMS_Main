using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Library.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace ATSCADA_Client.Pages.DashBoard
{
    public partial class UserList
    {
        private PagingParameters paging = new PagingParameters();
        private List<ApplicationUser>? ListUsers = new List<ApplicationUser>();
        private List<SystemRole> ListRoles = new List<SystemRole>();
        //private List<Service>? listService = new List<Service>();
        private List<Counter> listCounter = new List<Counter>();
        private string currentSortColumn = "Id";
        private bool isSortAscending = true;
        private string SearchBy { get; set; }
        public MetaData MetaData { get; set; } = new MetaData();
        //Handle Create - Update
        private bool isModalVisible = false;
        private bool isEditModalVisible = false;
        private bool isDeleteConfirmVisible = false; 
        private ApplicationUser newUser = new ApplicationUser();
        private ApplicationUser editUser = new ApplicationUser();
        //private string? selectedNameService;
        private string? JwtName;
        private string? ServiceDescription;
        private string? CounterName;
        private HubConnection? hubConnection;
        private async Task GetListUsers()
        {
            var listPagingResponse = await UserApiClient.GetAllUser(paging);
            ListUsers = listPagingResponse!.Items;
            MetaData = listPagingResponse.MetaData;
           
        }
        protected override async Task OnInitializedAsync()
        {
            //Cấu hình Paging
            paging.PageSize = 5;
            var pageAll = new PagingParameters();
            // Lấy dữ liệu ban đầu - Lấy dữ liệu từ Service để hiển thị Name Counter
            await GetListUsers();
            var counters = await CounterApiClient.GetAllCounter(pageAll);
            listCounter = counters.Items;
            //var Services = await QueueApiClient.GetServiceList();
            //listService = Services.Where(x => x.IsActive == true).ToList();
            ListRoles = await SystemRoleApiClient.GetAllSystemRole();
            // Lấy JWT Name cho người dùng hiện tại
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            JwtName = authState.User.Identity!.Name;
        }
        private async Task SelectedPage(int page)
        {
            paging.PageNumber = page;
            await GetListUsers();
        }

        private async void SortByColumn(string column)
        {
            if (currentSortColumn == column)
            {
                isSortAscending = !isSortAscending;
            }
            else
            {
                currentSortColumn = column;
                isSortAscending = true;
            }
            // Update the paging parameters with the new sort order
            paging.SortBy = currentSortColumn;
            paging.SortOrder = isSortAscending ? "asc" : "desc";

            // Reload the sorted data
            await GetListUsers();
        }
        private async void OnPageSizeChanged(int selectedPageSize)
        {
            await SortByEntries(selectedPageSize);
        }
        private async Task SortByEntries(int number)
        {
            paging.PageSize = number;
            await GetListUsers();
            StateHasChanged();
        }
        //private async void OnSearch(ChangeEventArgs e)
        //{
        //    paging.SearchTerm = e.Value?.ToString()!;
        //    await GetListUsers();
        //    StateHasChanged();
        //}
        private async void OnSearch(ChangeEventArgs e, string SearchBy)
        {
            paging.SearchTerm = e.Value?.ToString()!;
            paging.SearchBy = SearchBy;
            await GetListUsers();
            StateHasChanged();
        }

        private string GetNameCounter(int counterId)
        {
            var nameCounter = listCounter!.Where(x => x.Id == counterId).FirstOrDefault();
            if (nameCounter != null)
            {
                CounterName = nameCounter.Name;
                return CounterName!;
            }
            else return null!;
        }
        //Modal display Add New Counter
        private void OpenModal()
        {
            isModalVisible = true;
        }

        private Task CloseModal()
        {
            isModalVisible = false;
            isEditModalVisible = false;
            newUser = new ApplicationUser();
            return Task.CompletedTask;
        }

        private async Task HandleValidSubmitCreate()
        {
            //var nameService = newService.Name;

            var newUserDto = new ApplicationUser
            {
                FullName = newUser.FullName,
                UserName = newUser.UserName,
                Password = newUser.Password,
                CounterId = newUser.CounterId,
                SystemRoleId = newUser.SystemRoleId,
                CreatedDate = DateTime.Now,
                CreatedBy = JwtName
            };
            // Add new counter to the list
            var result = await UserApiClient.CreateNewUser(newUserDto);
            if (result)
            {
                await CloseModal(); // Close modal after submission
                ToastService.ShowSuccess("Added Successfully");
                await GetListUsers();
                StateHasChanged();
            }
            else ToastService.ShowError("Oops, have an error!");

        }
        // Method to open the edit modal and pre-fill the form with selected counter data
        private void OpenEditModal(ApplicationUser oldUser)
        {
            // Pre-fill the edit form with the selected counter data
            editUser = new ApplicationUser
            {
                Id = oldUser.Id,
                FullName = oldUser.FullName,
                UserName = oldUser.UserName,
                CounterId = oldUser.CounterId, 
                SystemRoleId = oldUser.SystemRoleId,
                LastUpdatedBy = JwtName,
            };
            isEditModalVisible = true; // Show the modal
        }
        private async Task HandleValidSubmitUpdate()
        {
            var result = await UserApiClient.UpdateUser(editUser);
            if (result)
            {
                await CloseModal();
                ToastService.ShowSuccess("Updated Successfully");
                await GetListUsers();
                StateHasChanged();
            }
            else ToastService.ShowError("Oops, there was an error!");

        }
        private void ShowDeleteConfirmation(ApplicationUser user)
        {
            editUser = user;
            isDeleteConfirmVisible = true;
        }
        private void CloseDeleteConfirmation()
        {
            isDeleteConfirmVisible = false;
        }
        private async void DeleteConfirmed()
        {
            if (editUser != null)
            {
                // Thực hiện hành động xóa
                await UserApiClient.DeleteUser(editUser);
                // Đóng modal sau khi xác nhận xóa
                CloseDeleteConfirmation();
                StateHasChanged();
            }
           
        }
        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }
        }
    }
}
