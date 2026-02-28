namespace LMS.Backend.Models.Settings
{
    public class SmtpSettings
    {
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public string User { get; set; } = null!;
        public string Pass { get; set; } = null!;
        public string FromName { get; set; } = "Kütüphane Mobil";
        public string FromEmail { get; set; } = null!;
    }
}
//Bu dosya, `appsettings.json` içindeki SMTP ayarlarını modele dönüştürmek için kullanılır
//ve `Program.cs` ile `SmtpEmailService` tarafından mail gönderme işlemlerinde kullanılır.
