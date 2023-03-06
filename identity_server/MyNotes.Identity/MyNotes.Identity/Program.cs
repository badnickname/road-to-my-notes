using Microsoft.EntityFrameworkCore;
using MyNotes.Identity;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseNLog();
builder.Configuration.AddJsonFile("appsettings.Development.json");
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();
builder.Services.AddDbContext<ApplicationContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
    options.UseOpenIddict();
});
builder.Services.AddControllers();
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<ApplicationContext>();
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
app.UseEndpoints(options =>
{
    options.MapControllers();
    options.MapDefaultControllerRoute();
});

app.Run();