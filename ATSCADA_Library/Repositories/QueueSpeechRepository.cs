using ATSCADA_Library.Data;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Interfaces.Server;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Repositories
{
    public class QueueSpeechRepository : IQueueSpeech
    {
        private readonly ATSCADADbContext _context;

        public QueueSpeechRepository(ATSCADADbContext context)
        {
            _context = context;
        }
        public async Task<QueueSpeech> CreateQueueAsync(QueueSpeech model)
        {
            _context.QueueSpeeches.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<List<QueueSpeech>> GetAllQueueSpeechAsync()
        {
            return await _context.QueueSpeeches.Where(x=>x.IsCompleted == false).OrderBy(x=>x.CreatedDate).ToListAsync();
        }

        public async Task<QueueSpeech> UpdateQueueSpeechAsync(QueueSpeech model)
        {
            var oldData = await _context.QueueSpeeches.FindAsync(model.Id);
            if (oldData != null)
            {
                oldData.IsCompleted = true;
                oldData.SpeechAt = DateTime.Now;
                _context.QueueSpeeches.Update(oldData);
                await _context.SaveChangesAsync();
                return oldData;
            }
            else return new QueueSpeech();
        }
    }
}
