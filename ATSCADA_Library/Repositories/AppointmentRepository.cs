using ATSCADA_Library.Data;
using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers;
using ATSCADA_Library.Interfaces.Server;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Repositories
{
    //public class AppointmentRepository : IAppointmentRepository
    //{
    //    private readonly ATSCADADbContext _context;
    //    public AppointmentRepository(ATSCADADbContext context)
    //    {
    //        _context = context;
    //    }
    //    public async Task<Appointment> CreateAppointmentAsync(AppointmentDto dto)
    //    {
    //        var appointment = new Appointment
    //        {
    //            Name = dto.Name,
    //            PhoneNumber = dto.PhoneNumber,
    //            AppointmentDate = dto.AppointmentDate,
    //            //AppointmentTime = dto.AppointmentTime.ToTimeSpan(),
    //            Message = dto.Message,
    //            ServiceId = dto.ServiceId,
    //        };
    //        var rs = appointment;
    //        _context.Add(appointment);

    //        await _context.SaveChangesAsync();
    //        return appointment;
    //    }

    //    public Task<Appointment> GetAppointmentsAsync()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public async Task<Appointment> GetLastAppointmentAsync(string phoneNumber)
    //    {
    //        var lastRecord = await _context.Appointments
    //        .Where(p => p.PhoneNumber == phoneNumber && p.Status == "Unverified" && p.Verified == false)
    //        .OrderByDescending(p => p.Id)
    //        .FirstOrDefaultAsync();
    //        return lastRecord!;
    //    }

    //    public async Task<Appointment> UpdateAppointmentAsync(Appointment appointment)
    //    {
    //        _context.Appointments.Update(appointment);
    //        await _context.SaveChangesAsync();
    //        return appointment;
    //    }
    //}
}

