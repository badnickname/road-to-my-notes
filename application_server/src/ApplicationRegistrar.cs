using System.Text.Json;
using MyNotes.Share;

namespace MyNotes.Application;

/// <summary>
///     Регистрирует приложение в identity_server
/// </summary>
public class ApplicationRegistrar : BackgroundService
{
    private readonly IHttpClientFactory _factory;
    private readonly ClientOptions _options;

    public ApplicationRegistrar(IHttpClientFactory factory, ClientOptions options)
    {
        _factory = factory;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var client = _factory.CreateClient(Constants.IdentityServiceApi);
        await client.PostAsJsonAsync(new Uri("clients"), _options, stoppingToken);
    }
}