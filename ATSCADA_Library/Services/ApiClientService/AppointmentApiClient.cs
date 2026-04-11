using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Interfaces.Client;
using Azure.Core;
using Humanizer;
using System.Net.Http;
using System.Net.Http.Json;

namespace ATSCADA_Client.Services
{
    public class AppointmentApiClient : IAppointmentApiClient
    {
        //public HttpClient? _httpClient;
        //public AppointmentApiClient(HttpClient? httpClient)
        //{
        //    _httpClient = httpClient;
        //}

        //public async Task<Appointment> CreateAppointment(AppointmentDto dto)
        //{
        //    //Send a POST request => status code 200
        //    var response = await _httpClient!.PostAsJsonAsync("api/Appointment", dto);
        //    // Ensure the request was successful
        //    response.EnsureSuccessStatusCode();
        //    var result = await response.Content.ReadFromJsonAsync<Appointment>();
        //    return result!;
        //}
        //public async Task<Appointment> GetAppointmentByPhone(string? phoneNumber)
        //{
        //    var result = await _httpClient!.GetFromJsonAsync<Appointment>($"api/Appointment/get-appointment-by-phoneNumber/{phoneNumber}");
        //    return result!;
        //}
        //public async Task<bool> AppointmentVerified(string? phoneNumber)
        //{
        //    var updateData = new AppointmentUpdateStatusRequest
        //    {
        //        Status = "Waiting",
        //        Verified = true
        //    };
        //    var response = await _httpClient!.PatchAsJsonAsync($"api/Appointment/update-status-appointment-verified/{phoneNumber}", updateData);
        //    return response.IsSuccessStatusCode;
        //}


    }
}
