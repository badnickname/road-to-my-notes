using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyNotes.Identity;
using NLog.Web;
using Polly;
using Polly.Extensions.Http;
using Quartz;

#if DEBUG
const string env = "Development";
#else
const string env = "Production";
#endif

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseNLog();
builder.Configuration.AddJsonFile("appsettings.json");
builder.Configuration.AddJsonFile($"appsettings.{env}.json", true);
builder.Services.AddTransient<EmailService>();
builder.Services.AddHttpClient(Constants.EmailServiceApi)
    .ConfigureHttpClient(client =>
        client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("EmailService:Url")))
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode != HttpStatusCode.OK)
        .WaitAndRetryAsync(5, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews();
builder.Services.AddLogging();
builder.Services.AddAuthentication()
    .AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>(Constants.AuthenticationScheme, options =>
    {
        options.Login = builder.Configuration.GetValue<string>("AuthenticationOptions:Login");
        options.Password = builder.Configuration.GetValue<string>("AuthenticationOptions:Password");
    });
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationContext>()
    .AddDefaultTokenProviders();
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
        options.SetAuthorizationEndpointUris("connection/authorize")
            .SetTokenEndpointUris("connection/token");
        options.IgnoreScopePermissions();
        options.AllowAuthorizationCodeFlow()
            .AllowRefreshTokenFlow();
        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();
        options.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .DisableTransportSecurityRequirement();
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
app.UseEndpoints(options => options.MapControllers());

app.Run();