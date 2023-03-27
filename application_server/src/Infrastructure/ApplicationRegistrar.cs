using Microsoft.Extensions.Options;
using MyNotes.Share;

namespace MyNotes.Application.Infrastructure;

/// <summary>
///     Регистрирует приложение в identity_server
/// </summary>
public class ApplicationRegistrar : BackgroundService
{
    private readonly IHttpClientFactory _factory;
    private readonly IOptions<ClientOptions> _options;

    public ApplicationRegistrar(IHttpClientFactory factory, IOptions<ClientOptions> options)
    {
        _factory = factory;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(5000, stoppingToken);
        var client = _factory.CreateClient(Constants.IdentityServiceApi);
        var options = _options.Value;
        await client.PostAsJsonAsync(new Uri("clients", UriKind.Relative), options, stoppingToken);
    }
}