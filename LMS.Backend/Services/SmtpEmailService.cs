using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using LMS.Backend.Models.Settings;

namespace LMS.Backend.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpSettings _smtp;

        public SmtpEmailService(IOptions<SmtpSettings> smtpOptions)
        {
            _smtp = smtpOptions.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string bodyText)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtp.FromName, _smtp.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            message.Body = new TextPart("plain") { Text = bodyText };

            using var client = new SmtpClient();

            
            await client.ConnectAsync(_smtp.Host, _smtp.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtp.User, _smtp.Pass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}

//Bu dosya, `appsettings.json` içindeki `SmtpSettings` ayarlarını kullanarak e-posta g
//önderir ve şifre sıfırlama işlemlerinde `StudentsController` ile ve `IEmailService`
//arayüzü üzerinden DI sistemiyle bağlantılı çalışır.
