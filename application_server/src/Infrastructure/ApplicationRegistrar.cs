using Microsoft.Extensions.Options;
using MyNotes.Share;

namespace MyNotes.Application.Infrastructure;

/// <summary>
///     Регистрирует приложение в identity_server
/// </summary>
public class ApplicationRegistrar : BackgroundService
{
    private readonly IHttpClientFactory _factory;
    private readonly IOptions<IdentityOptions> _options;

    public ApplicationRegistrar(IHttpClientFactory factory, IOptions<IdentityOptions> options)
    {
        _factory = factory;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(5000, stoppingToken);
        var client = _factory.CreateClient(Constants.IdentityServiceApi);
        var clientOptions = _options.Value.ApplicationClient;
        var serverOptions = _options.Value.ApplicationServer;
        await client.PostAsJsonAsync(new Uri("clients", UriKind.Relative), clientOptions, stoppingToken);
        await client.PostAsJsonAsync(new Uri("clients", UriKind.Relative), serverOptions, stoppingToken);
    }
}