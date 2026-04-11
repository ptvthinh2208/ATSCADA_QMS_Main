using ATSCADA_Library.Data;
using ATSCADA_Library.DTOs;
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
    public class ZnsConfigRepository : IZnsConfigRepository
    {
        private readonly ATSCADADbContext _context;
        public ZnsConfigRepository(ATSCADADbContext context) 
        {
            _context = context;
        }
        public async Task<ZnsConfig> GetZnsConfigAsync()
        {
            var result = await _context.ZnsConfigs.FirstOrDefaultAsync();
            return result!;
        }

        public Task<ZnsTemplate> GetZnsTemplateAsync()
        {
            throw new NotImplementedException();
        }
        public async Task<List<ZnsInfoDto>> GetZnsInfoAsync()
        {
            var result = await (from template in _context.ZnsTemplates
                                join config in _context.ZnsConfigs
                                on template.ZnsConfigId equals config.Id
                                select new ZnsInfoDto
                                {
                                    TemplateID = template.TemplateID,
                                    TemplateName = template.TemplateName,
                                    AccessToken = config.AccessToken,
                                    RefreshToken = config.RefreshToken,
                                    AppID = config.AppID,
                                    SecretKey = config.SecretKey,
                                }).ToListAsync();

            return result;
        }

        public async Task UpdateZnsConfigAsync(ZnsInfoDto znsInfoDto)
        {
            var existingZnsConfig = await _context.ZnsConfigs.FirstOrDefaultAsync();
            // Chỉ cập nhật những thuộc tính có giá trị (không phải null)
            if (znsInfoDto.AccessToken != null)
            {
                existingZnsConfig!.AccessToken = znsInfoDto.AccessToken;
            }

            if (znsInfoDto.RefreshToken != null)
            {
                existingZnsConfig!.RefreshToken = znsInfoDto.RefreshToken;
            }
            if (znsInfoDto.AppID != null)
            {
                existingZnsConfig!.AppID = znsInfoDto.AppID;
            }
            if (znsInfoDto.SecretKey != null)
            {
                existingZnsConfig!.SecretKey = znsInfoDto.SecretKey;
            }
            _context.ZnsConfigs.Update(existingZnsConfig!);
            _context.SaveChanges();
        }
    }
}
