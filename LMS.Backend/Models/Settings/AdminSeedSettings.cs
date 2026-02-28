namespace LMS.Backend.Models.Settings
{
    public class AdminSeedSettings
    {
        public bool Enabled { get; set; } = false;
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string FullName { get; set; } = "Admin";
    }
}

//Bu dosya, admin seed işlemi için gerekli ayarları `appsettings.Development.json`’dan okumak
//üzere tanımlar ve `Program.cs` ile `DbSeeder` sınıfı tarafından kullanılır.
