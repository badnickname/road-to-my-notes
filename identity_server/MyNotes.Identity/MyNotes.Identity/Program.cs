using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyNotes.Identity;
using NLog.Web;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;

#if DEBUG
const string env = "Development";
#else
const string env = "Production";
#endif

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseNLog();
builder.Configuration.AddJsonFile("appsettings.json");
builder.Configuration.AddJsonFile($"appsettings.{env}.json", true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();
builder.Services.AddDbContext<ApplicationContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
    options.UseOpenIddict();
});
builder.Services.AddQuartz(options =>
{
    options.UseMicrosoftDependencyInjectionJobFactory();
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();
});
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<ApplicationContext>();
        options.UseQuartz();
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("connect/token");
        options.AllowClientCredentialsFlow();
        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();
        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapMethods("/connect/token", new[] { "GET", "POST" }, CreateConnectMethod(app.Services))
    .WithName("Token");

app.Run();

static Func<HttpContext, Task<ActionResult>> CreateConnectMethod(IServiceProvider provider)
{
    return async context =>
    {
        var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();

        var request = context.GetOpenIddictServerRequest()!;
        if (!request.IsClientCredentialsGrantType()) throw new NotImplementedException("Grant Type не реализован");

        var application = await manager.FindByClientIdAsync(request.ClientId);
        if (application is null) throw new InvalidOperationException("Client не зарегистирован");

        var identity = new ClaimsIdentity(
            TokenValidationParameters.DefaultAuthenticationType,
            Claims.Name,
            Claims.Role);

        identity.SetClaim(Claims.Subject, await manager.GetClientIdAsync(application));
        identity.SetClaim(Claims.Name, await manager.GetDisplayNameAsync(application));
        identity.SetDestinations(static claim => claim.Type switch
        {
            Claims.Name or Claims.Subject when claim.Subject!.HasScope(Scopes.Profile)
                => new[] { Destinations.AccessToken, Destinations.IdentityToken },
            _ => new[] { Destinations.AccessToken }
        });

        return new SignInResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    };
}