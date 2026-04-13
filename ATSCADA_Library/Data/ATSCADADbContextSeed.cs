using ATSCADA_Library.Entities;

namespace ATSCADA_Library.Data
{
    public static class ATSCADADbContextSeed
    {

        public static async Task SeedAsync(ATSCADADbContext context)
        {
            //Tạo dữ liệu SystemRole
            if (!context.SystemRoles.Any())
            {
                var roles = new List<SystemRole>
                {
                    new SystemRole{ Id = 1, Name = "Admin", CreatedBy = "System", CreatedDate = DateTime.Now, Description = "Has access to all Counters, all data reports and can edit Counter settings" },
                    new SystemRole{ Id = 2, Name = "Manager", CreatedBy = "System", CreatedDate = DateTime.Now, Description = "Can serve visitors and has access to personal data reports" }
                };
                context.SystemRoles.AddRange(roles);
            }
            //Tạo dữ liệu ZnsConfigs
            if (!context.ZnsConfigs.Any())
            {
                var znsConfig = new ZnsConfig()
                {
                    Id = 1,
                    AccessToken = "AVx9MWmEG3CazyzS30L4DINonrm270HAHvNAQ0Pu25nOZlrI3WKqJrYzotyR5obT9hl_L78X4qS9iC99SK0WMXdqsbvjRWn6JkI7MpngHLzksfSyEInBAd3KbnaO8KyANehDQHix3rjxfFnJMm0mMXhmwbSx0mj2MFRQOmbJE5TjziurB5eR6LVec2GJP7j6JT3jJYncCqrfbUjGA3msJ6w8j5KHAtjRJDcGH11DHYPPrOHG3dnHE7h_dYS_T58bUUU_23DbI6fYuxXx7s5wVtdej65OVaLj1UwRGd9tJ59I_w5M6mqYQ3cqtcPB6WTLBv3VQreI1WSbbkyq604aBcEJtW8U30OVIvpLCGifDbb9qU1I5t1sQaBd_LWYIZT1TzE-MIDPV7TSxgr05KnOGdR9Zt5CRX30kriD440q",
                    RefreshToken = "CvaV77U2G5q-a1aQTub4Mp-L4Hfva4WnRVaNAqhaFMrukMWfREXt5NVxNLTUbmHzVjS2MJlDFqaTosT-GOzFOWI9R790u7jJVPuXPsNM72CZYWLY9yac8GNJUJGsb5qf8ey3FXJF60XLZ3yoJPzcSsg3SZ5rnriAQje03qt0F4zAZ60BGlrW3WgbJn92-sGlG-PzRNw6H75rxtalT_DSBMUv06rUwXXq5uKvPWlK5oDNcXLdH8H0MWsQVX1FsbeHIx1_06h3IY9JdcyQHCPB2WQAHpCIvNaXKgn6EWxYPHuPWpWWNTmd0qR36pD1XmzsU_ORGMok7MjCrY5TKxOsNXhJ0bS6fpPlFRXYRnRGLYu8h6qb6-5KF2-OV3OE_tynB-1wRp_pRr03h3114-HlUmgrLqSahWjZKt4hRNDsd41Q",
                    AppID = "2550228944820297827",
                    SecretKey = "5yTeyv22WpS4r7b5iu07",
                };
                context.ZnsConfigs.Add(znsConfig);
            }
            //Tạo dữ liệu Settings
            if (!context.Settings.Any())
            {
                var setting = new Setting()
                {
                    Id = 1,
                    IsAppointmentForm = false,
                    IsActiveSendZNS = false,
                    EstimatedCompletionTime = 10,
                    FooterTextFontSize = 24,
                    MaxVisibleCounters = 1

                };
                context.Settings.Add(setting);
            }
            //Tạo dữ liệu ZnsTemplate
            if (!context.ZnsTemplates.Any())
            {
                var znsTemplates = new List<ZnsTemplate>()
                {
                    new ZnsTemplate{Id = 1, TemplateID = "365542",TemplateName = "Template_Confirm_ID",ZnsConfigId = 1, },
                    new ZnsTemplate{Id = 2, TemplateID = "361455",TemplateName = "Template_Notification_ID",ZnsConfigId = 1, },
                };
                context.ZnsTemplates.AddRange(znsTemplates);
            }
            //Tạo dữ liệu WorkShift
            if (!context.WorkShifts.Any())
            {
                var workShifts = new List<WorkShift>()
                {
                    new WorkShift{Id = 1, NameWorkShift = "Ca Sáng",StartTime = new TimeSpan(8, 0, 0),EndTime = new TimeSpan(12, 0, 0), isActive = true },
                    new WorkShift{Id = 2, NameWorkShift = "Ca Chiều",StartTime = new TimeSpan(13, 0, 0),EndTime = new TimeSpan(17, 0, 0), isActive = true },
                };
                context.WorkShifts.AddRange(workShifts);
            }
            //Tạo dữ liệu đầu cho Application User
            if (!context.ApplicationUsers.Any())
            {
                var users = new ApplicationUser()
                {
                    Id = 1,
                    FullName = "ATSCADA",
                    UserName = "admin",
                    Password = BCrypt.Net.BCrypt.HashPassword("ATPro1234560"),
                    SystemRoleId = 1, //Admin
                    CounterId = 0
                };
                context.ApplicationUsers.Add(users);
            }
            await context.SaveChangesAsync();
        }
    }
}

