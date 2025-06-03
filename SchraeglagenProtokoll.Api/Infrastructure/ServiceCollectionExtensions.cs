using SchraeglagenProtokoll.Api.Infrastructure.EMail;

namespace SchraeglagenProtokoll.Api.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static void AddEmail(this IServiceCollection services, IConfiguration configuration)
    {
        // Register SMTP options using the .NET options pattern
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));

        // Register SMTP email service using options pattern
        services.AddSingleton<IEmailService, SmtpEmailService>();
    }
}
