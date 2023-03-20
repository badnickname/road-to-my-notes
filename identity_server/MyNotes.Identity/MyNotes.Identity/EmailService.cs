using System.Text.Json;

namespace MyNotes.Identity;

public sealed class EmailService
{
    private readonly IHttpClientFactory _factory;
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger, IHttpClientFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    public void Send(string email, string topic, string body)
    {
        try
        {
            _logger.LogInformation("{Email} {Topic} {Body}", email, topic, body);

            var client = _factory.CreateClient(Constants.EmailServiceApi);

            var request = new HttpRequestMessage(HttpMethod.Put, "emails");
            var payload = JsonSerializer.Serialize(new
            {
                Email = email,
                Topic = topic,
                Body = body
            });
            request.Content = new StringContent(payload);

            client.Send(request);
        }
        catch (Exception e)
        {
            _logger.LogError("{Error}", e);
        }
    }
}