using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Client.Services.ApiClientService;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using Microsoft.AspNetCore.Components;

namespace ATSCADA_Client.Pages.DashBoard
{
    public partial class ServiceList
    {
        private List<Service>? listService = new List<Service>();
        private PagingParameters paging = new PagingParameters();

        private string currentSortColumn = "Id";
        private bool isSortAscending = true;
        public MetaData MetaData { get; set; } = new MetaData();

        //Handle Create - Update
        private bool isModalVisible = false;
        private bool isEditModalVisible = false;
        private Service newService = new Service();
        private ServiceDto editService = new ServiceDto();
        //private string? selectedNameService;
        private string? JwtName;
        protected override async Task OnInitializedAsync()
        {
            paging.PageSize = 5;
            await GetServices();

            // Get the authentication state and extract the JWT name (username)
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            JwtName = authState.User.Identity!.Name;
        }
        private async void ToggleIsActive(Service item)
        {
            try
            {
                item.IsActive = !item.IsActive;
                item.LastUpdatedBy = JwtName;
                await QueueApiClient.UpdateStatusService(item.Id, item);
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., logging)
                Console.WriteLine($"Error updating item status: {ex.Message}");
            }
        }
        private async Task GetServices()
        {
            var listPagingResponse = await QueueApiClient.GetServiceList(paging);
            listService = listPagingResponse!.Items;
            MetaData = listPagingResponse.MetaData;
        }
        private async Task SelectedPage(int page)
        {
            paging.PageNumber = page;
            await GetServices();
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
            await GetServices();
        }
        private async void OnPageSizeChanged(int selectedPageSize)
        {
            await SortByEntries(selectedPageSize);
        }
        private async Task SortByEntries(int number)
        {
            paging.PageSize = number;
            await GetServices();
            StateHasChanged();
        }
        private async void OnSearch(ChangeEventArgs e)
        {
            paging.SearchTerm = e.Value?.ToString()!;
            await GetServices();
            StateHasChanged();
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
            newService = new Service();
            return Task.CompletedTask;
        }

        private async Task HandleValidSubmitCreate()
        {
            // Get the selected User ID and Queue ID from counterDto
            var nameService = newService.Name;
            
            newService = new Service
            {
                Name = newService.Name,
                Description = newService.Description,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = JwtName
            };
            // Add new counter to the list
            var result = await QueueApiClient.CreateService(newService);
            if (result)
            {
                await CloseModal(); // Close modal after submission
                ToastService.ShowSuccess("Added Successfully");
                await GetServices();
                StateHasChanged();
            }
            else ToastService.ShowError("Oops, have an error!");

        }
        // Method to open the edit modal and pre-fill the form with selected counter data
        private void OpenEditModal(Service oleService)
        {
            // Pre-fill the edit form with the selected counter data
            editService = new ServiceDto
            {
                Id = oleService.Id,
                Name = oleService.Name,
                Description = oleService.Description,
                LastUpdatedBy = JwtName,
                LastUpdated = DateTime.Now
            };
            isEditModalVisible = true; // Show the modal
        }
        private async Task HandleValidSubmitUpdate()
        {
            var result = await QueueApiClient.UpdateService(editService.Id,editService);
            if (result)
            {
                await CloseModal(); // Close modal after submission
                ToastService.ShowSuccess("Updated Successfully");
                await GetServices();
                StateHasChanged();
            }
            else ToastService.ShowError("Oops, there was an error!");

        }
    }
}
