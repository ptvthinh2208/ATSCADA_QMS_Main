using ATSCADA_Client.Helpers.Pagination;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using Microsoft.AspNetCore.Components;

namespace ATSCADA_Client.Pages.DashBoard
{
    public partial class CounterList
    {
        private List<Counter>? ListCounter = new List<Counter>();
        private List<Service>? listService = new List<Service>();
        private PagingParameters paging = new PagingParameters();

        private string? selectedNameService;
        private string? selectedDescriptionService;
        private string? JwtName;
        private string currentSortColumn = "Id";
        private bool isSortAscending = true;
        private string? ServiceDescription;
        //Handle Create - Update
        private bool isModalVisible = false;
        private bool isEditModalVisible = false;
        
        private CounterDto newCounter = new CounterDto();
        private CounterDto editCounter = new CounterDto();
        public MetaData MetaData { get; set; } = new MetaData();
        private int countServiceAvailable;
        protected override async Task OnInitializedAsync()
        {
            paging.PageSize = 5;
            await GetCounterList();
            var Services = await QueueApiClient.GetServiceList();
            listService = Services.Where(x => x.IsActive == true).ToList();
            countServiceAvailable = GetAvailableServices().Count();
            // Get the authentication state and extract the JWT name (username)
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            JwtName = authState.User.Identity!.Name;
        }
        //private int TotalRecords => ListCounter!.Count;
        //private int FromRecord => (paging.PageNumber - 1) * paging.PageSize + 1;
        //private int ToRecord => Math.Min(paging.PageNumber * paging.PageSize, TotalRecords);
        private async void ToggleIsActive(Counter item)
        {
            try
            {
                item.IsActive = !item.IsActive;
                editCounter.IsActive = item.IsActive;
                editCounter.Id = item.Id;
                editCounter.ServiceId = item.ServiceID;
                editCounter.NameCounter = item.Name;
                editCounter.Code = item.Code;
                editCounter.ServiceDescription = item.ServiceDescription;
                editCounter.ServiceName = item.ServiceName;
                await CounterApiClient.UpdateCounter(editCounter);
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., logging)
                Console.WriteLine($"Error updating item status: {ex.Message}");
            }
        }
        private async Task GetCounterList()
        {
            var listPagingResponse = await CounterApiClient.GetAllCounter(paging);
            ListCounter = listPagingResponse!.Items;
            MetaData = listPagingResponse.MetaData;
        }
        private async Task SelectedPage(int page)
        {
            paging.PageNumber = page;
            await GetCounterList();
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
            await GetCounterList();
        }
        private async void OnPageSizeChanged(int selectedPageSize)
        {
            await SortByEntries(selectedPageSize);
        }
        private async Task SortByEntries(int number)
        {
            paging.PageSize = number;
            await GetCounterList();
            StateHasChanged();
        }
        private async void OnSearch(ChangeEventArgs e)
        {
            paging.SearchTerm = e.Value?.ToString()!;
            await GetCounterList();
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
            newCounter = new CounterDto();
            return Task.CompletedTask;
        }

        private async void HandleValidSubmitCreate()
        {
            if (newCounter.ServiceId != 0)
            {
                // Get the selected User ID and Queue ID from counterDto
                //var nameCounter = newCounter.NameCounter;
                var selectedServiceId = newCounter.ServiceId;
                var selectedService = listService!.FirstOrDefault(queue => queue.Id == selectedServiceId);
                if (selectedService != null)
                {
                    //Tiếp đầu ngữ ( Tên của dịch vụ )
                    selectedNameService = selectedService.Name!;
                    selectedDescriptionService = selectedService.Description!;
                }
                newCounter = new CounterDto
                {
                    ServiceId = selectedServiceId,
                    NameCounter = newCounter.NameCounter,
                    Code = newCounter.Code,
                    ServiceName = selectedNameService,
                    ServiceDescription = selectedDescriptionService,
                    CreatedBy = JwtName
                };
                // Add new counter to the list
                var result = await CounterApiClient.CreateNewCounter(newCounter);
                if (result != null)
                {
                    await CloseModal(); // Close modal after submission
                    await GetCounterList();
                    StateHasChanged();
                    ToastService.ShowSuccess("Added Successfully");
                }
                else ToastService.ShowError("A counter with this name already exists.");
            }
            else ToastService.ShowError("You have not selected a service");
            
        }
        // Method to open the edit modal and pre-fill the form with selected counter data
        private void OpenEditModal(Counter oldCounter)
        {
            // Pre-fill the edit form with the selected counter data
            editCounter = new CounterDto
            {
                Id = oldCounter.Id,
                NameCounter = oldCounter.Name,
                Code = oldCounter.Code,
                ServiceId = oldCounter.ServiceID,
                ServiceName = selectedNameService ?? string.Empty,
                ServiceDescription = selectedDescriptionService,
                LastUpdatedBy = JwtName,
                LastUpdated = DateTime.Now,
                //CreatedBy = oldCounter.CreatedBy,
            };
            isEditModalVisible = true; // Show the modal
        }
        private async void HandleValidSubmitUpdate()
        {
            if(editCounter.ServiceId != 0)
            {
                var selectedServiceId = editCounter.ServiceId;
                var selectedService = listService!.FirstOrDefault(queue => queue.Id == selectedServiceId);
                if (selectedService != null)
                {
                    //Tiếp đầu ngữ ( Tên của dịch vụ )
                    selectedNameService = selectedService.Name!;
                    selectedDescriptionService = selectedService.Description!;
                }
                editCounter.ServiceName = selectedNameService;
                editCounter.ServiceDescription = selectedDescriptionService;
                // Add new counter to the list
                var result = await CounterApiClient.UpdateCounter(editCounter);
                if (result != null)
                {
                    await CloseModal(); // Close modal after submission
                    await GetCounterList();
                    StateHasChanged();
                    ToastService.ShowSuccess("Updated Successfully");
                }
                else ToastService.ShowError("A counter with this name already exists.");
            }
            else ToastService.ShowError("You have not selected a service");

        }
        private string GetDescriptionService(long ServiceId)
        {
            var Service = listService!.Where(x=>x.Id == ServiceId).FirstOrDefault();
            if (Service != null)
            {
                ServiceDescription = Service.Description;
                return ServiceDescription!;
            }
            else return null!;
        }
        private IEnumerable<Service> GetAvailableServices()
        {
            var assignedServiceIds = ListCounter!.Select(c => c.ServiceID).ToList();
            return listService!.Where(q => !assignedServiceIds.Contains(q.Id)).ToList();
        }

    }
}
