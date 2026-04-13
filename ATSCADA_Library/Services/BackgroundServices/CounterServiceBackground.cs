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
    public class CounterServiceBackground
    {
        private readonly ATSCADADbContext _dbContext;

        public CounterServiceBackground(ATSCADADbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task UpdateTotalCountForDateAsync(List<Queue> appointments, CancellationToken cancellationToken)
        {
            foreach (var appointment in appointments)
            {
                // Cập nhật TotalCount của các counters tương ứng với ngày đặt lịch
                var counter = _dbContext.Counters.FirstOrDefault(c => c.Id == appointment.CounterId);
                if (counter != null)
                {
                    counter.TotalCount += 1; // Tăng TotalCount cho Counter tương ứng
                }
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

}
