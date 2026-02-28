using LMS.Backend.Services;
using LMS.Backend.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LMS.Backend.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LMSDbContext>();
            var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
            var admin = scope.ServiceProvider.GetRequiredService<IOptions<AdminSeedSettings>>().Value;

            
            await db.Database.MigrateAsync();

           
            if (!admin.Enabled) return;

            
            if (string.IsNullOrWhiteSpace(admin.Email) || string.IsNullOrWhiteSpace(admin.Password))
                return;

            
            var exists = await db.Students.AnyAsync(s => s.Email == admin.Email);
            if (exists) return;

            db.Students.Add(new LMS.Backend.Models.Student
            {
                FullName = string.IsNullOrWhiteSpace(admin.FullName) ? "Admin" : admin.FullName,
                Email = admin.Email,
                PasswordHash = passwordService.HashPassword(admin.Password),
                Role = "Admin"
            });

            await db.SaveChangesAsync();
        }
    }
}

//Bu dosya, uygulama başlarken veritabanı migration’larını uygular ve `appsettings`’ten gelen
//`AdminSeedSettings` bilgileriyle admin kullanıcıyı oluşturur; `Program.cs`, `LMSDbContext`,
//`IPasswordService` ve `Student` modeliyle bağlantılı çalışır.
