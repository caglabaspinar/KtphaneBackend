namespace LMS.Backend.Services
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string bodyText);
    }
}

//Bu dosya, uygulamada “mail gönderme” işini tanımlayan bir sözleşmedir; maili nasıl
//göndereceğimizi değil, sadece gönderme işleminin olacağını belirtir.
