namespace SchraeglagenProtokoll.Api.Infrastructure.EMail;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}
