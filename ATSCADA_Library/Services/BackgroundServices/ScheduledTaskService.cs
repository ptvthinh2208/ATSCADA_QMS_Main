using ATSCADA_Library.Data;
using ATSCADA_Library.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATSCADA_Library.Services.BackgroundServices
{
    public class ScheduledTaskService
    {
        private readonly ATSCADADbContext _dbContext;

        public ScheduledTaskService(ATSCADADbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Setting?> GetSettingsAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Settings.FirstOrDefaultAsync(cancellationToken);
        }
        public async Task<TimeSpan> GetInitialDelayAsync(CancellationToken stoppingToken)
        {
            var setting = _dbContext.Settings.FirstOrDefault();
            if (setting == null)
            {
                setting = new Setting
                {
                    ScheduledTaskTime = TimeSpan.FromHours(0), // Default to midnight
                };
                _dbContext.Settings.Add(setting);
                await _dbContext.SaveChangesAsync(stoppingToken);
            }

            var now = DateTime.Now;
            var nextExecution = DateTime.Today.Add(setting.ScheduledTaskTime.Value);

            if (now > nextExecution)
            {
                nextExecution = nextExecution.AddDays(1);
            }

            return nextExecution - now;
        }
    }

}
