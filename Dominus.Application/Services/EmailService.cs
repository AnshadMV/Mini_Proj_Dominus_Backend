using Dominus.Application.Interfaces.IServices;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var host = _config["Smtp:Host"];
        var port = int.Parse(_config["Smtp:Port"]);
        var email = _config["Smtp:Email"];
        var password = _config["Smtp:Password"];
        var enableSsl = bool.Parse(_config["Smtp:EnableSsl"]);

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(email, password),
            EnableSsl = enableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        var mail = new MailMessage
        {
            From = new MailAddress(email, "Dominus Support"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mail.To.Add(to);

        await client.SendMailAsync(mail);
    }

}
