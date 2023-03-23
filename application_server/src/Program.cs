using MyNotes.Application;
using MyNotes.Share;
using OpenIddict.Client;
using Polly;
using Polly.Extensions.Http;

#if DEBUG
const string env = "Development";
#else
const string env = "Production";
#endif

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json");
builder.Configuration.AddJsonFile($"appsettings.{env}.json", true);
builder.Services.Configure<ClientOptions>(builder.Configuration.GetSection("IdentityClient"));
builder.Services.AddHttpClient(Constants.IdentityServiceApi)
    .ConfigureHttpClient(client =>
    {
        var config = builder.Configuration.GetSection("IdentityService").Get<IdentityServiceOptions>();
        client.BaseAddress = new Uri(config.Url);
    })
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(5, attempt => TimeSpan.FromSeconds(Math.Pow(3, attempt))));
builder.Services
    .AddOpenIddict()
    .AddClient(options =>
    {
        var client = builder.Configuration.GetSection("IdentityClient").Get<ClientOptions>()!;
        var server = builder.Configuration.GetSection("IdentityService").Get<IdentityServiceOptions>()!;

        options.AllowAuthorizationCodeFlow()
            .AllowRefreshTokenFlow();

        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        options.AddRegistration(new OpenIddictClientRegistration
        {
            Issuer = new Uri(server.Url, UriKind.Absolute),
            ClientId = client.ClientId,
            RedirectUri = new Uri(client.RedirectUrl, UriKind.Absolute)
        });
    });
builder.Services.AddHostedService<ApplicationRegistrar>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();