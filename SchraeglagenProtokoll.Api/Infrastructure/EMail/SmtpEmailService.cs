using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace SchraeglagenProtokoll.Api.Infrastructure.EMail;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpClient _smtpClient;
    private readonly string _from;

    public SmtpEmailService(IOptions<SmtpOptions> options)
    {
        var optionsValue = options.Value;
        _from = optionsValue.From;
        _smtpClient = new SmtpClient(optionsValue.Host, optionsValue.Port)
        {
            Credentials = new NetworkCredential(optionsValue.From, optionsValue.Password),
        };
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        using var message = new MailMessage(_from, to, subject, body);
        await _smtpClient.SendMailAsync(message);
    }
}
