using MyNotes.Application;
using OpenIddict.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddOpenIddict()
    .AddClient(options =>
    {
        options.AllowAuthorizationCodeFlow()
            .AllowRefreshTokenFlow();

        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        options.AddRegistration(new OpenIddictClientRegistration
        {
            // todo
        });
    });
builder.Services.AddHostedService<ApplicationRegistrar>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();