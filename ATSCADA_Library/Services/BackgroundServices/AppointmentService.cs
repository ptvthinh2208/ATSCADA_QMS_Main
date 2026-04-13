using ATSCADA_Library.Data;
using ATSCADA_Library.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Services.BackgroundServices
{
    public class AppointmentService
    {
        private readonly ATSCADADbContext _dbContext;

        public AppointmentService(ATSCADADbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Queue>> GetAppointmentsForTodayAsync(CancellationToken cancellationToken)
        {
            var today = DateTime.Now.Date;
            return await _dbContext.Queues
                .Where(a => a.AppointmentDate.Date == today).ToListAsync(cancellationToken);
                
        }
    }

}
