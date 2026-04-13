using ATSCADA_Library.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATSCADA_Library.Data
{
    public class ATSCADADbContext (DbContextOptions<ATSCADADbContext> options) : DbContext(options)
    {
        //Data
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Queue> Queues { get; set; }
        public DbSet<QueueHistory> QueueHistories { get; set; }
        public DbSet<QueueSpeech> QueueSpeeches { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Counter> Counters { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<SystemRole> SystemRoles { get; set; }
        public DbSet<ZaloSmsHistory> ZaloSmsHistories { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Report> Reports { get; set; }

        //Setting
        public DbSet<Setting> Settings { get; set; }
        public DbSet<WorkShift> WorkShifts { get; set; }
        public DbSet<ZnsConfig> ZnsConfigs { get; set; }
        public DbSet<ZnsTemplate> ZnsTemplates { get; set; }
        public DbSet<Modbus> Modbuses { get; set; }
    }

}
