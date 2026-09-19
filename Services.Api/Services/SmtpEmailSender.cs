using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Services.Domain.Entities;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class SmtpEmailSender : IEmailSender<ApplicationUser>
{
    private readonly IConfiguration _configuration;

    public SmtpEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var smtpServer = _configuration["SmtpSettings:Server"];
        var port = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
        var senderName = _configuration["SmtpSettings:SenderName"];
        var senderEmail = _configuration["SmtpSettings:SenderEmail"];
        var password = _configuration["SmtpSettings:Password"];

        using var client = new SmtpClient(smtpServer, port)
        {
            Credentials = new NetworkCredential(senderEmail, password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail!, senderName),
            Subject = "Confirma tu cuenta en Servicios Ya!",
            Body = $"Hola {user.FullName}, por favor confirma tu cuenta haciendo clic en el siguiente enlace: <a href='{confirmationLink}'>Confirmar cuenta</a>",
            IsBodyHtml = true
        };

        mailMessage.To.Add(email);
        await client.SendMailAsync(mailMessage);
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var smtpServer = _configuration["SmtpSettings:Server"];
        var port = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
        var senderName = _configuration["SmtpSettings:SenderName"];
        var senderEmail = _configuration["SmtpSettings:SenderEmail"];
        var password = _configuration["SmtpSettings:Password"];

        using var client = new SmtpClient(smtpServer, port)
        {
            Credentials = new NetworkCredential(senderEmail, password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail!, senderName),
            Subject = "Recupera tu contraseña en Servicios Ya!",
            Body = $"Hola {user.FullName}, por favor haz clic en el siguiente enlace para restablecer tu contraseña: <a href='{resetLink}'>Restablecer contraseña</a>",
            IsBodyHtml = true
        };

        mailMessage.To.Add(email);
        await client.SendMailAsync(mailMessage);
    }
    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) => Task.CompletedTask;
}