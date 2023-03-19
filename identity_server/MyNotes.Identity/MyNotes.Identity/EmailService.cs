namespace MyNotes.Identity;

public sealed class EmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public void Send(string email, string topic, string body)
    {
        _logger.LogInformation("{Email} {Topic} {Body}", email, topic, body);
    }
}