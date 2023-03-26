using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using MyNotes.Application.Infrastructure;
using MyNotes.Share;
using OpenIddict.Validation.AspNetCore;
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
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.Login}:{config.Password}")));
    })
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(5, attempt => TimeSpan.FromSeconds(Math.Pow(3, attempt))));
builder.Services
    .AddOpenIddict()
    .AddValidation(options =>
    {
        var client = builder.Configuration.GetSection("IdentityClient").Get<ClientOptions>()!;
        var server = builder.Configuration.GetSection("IdentityService").Get<IdentityServiceOptions>()!;

        options.SetIssuer(server.Url);

        options.UseIntrospection()
            .SetClientId(client.ClientId)
            .SetClientSecret(client.ClientSecret);
        
        options.UseAspNetCore();

        options.UseSystemNetHttp();
    });
builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();
builder.Services.AddHostedService<ApplicationRegistrar>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/",  [Authorize] () => "Hello World!");

app.Run();