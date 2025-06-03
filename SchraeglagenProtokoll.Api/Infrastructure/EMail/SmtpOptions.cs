namespace SchraeglagenProtokoll.Api.Infrastructure.EMail
{
    public class SmtpOptions
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 1025;
        public string From { get; set; } = "test@example.com";
        public string Password { get; set; } = string.Empty;
    }
}
