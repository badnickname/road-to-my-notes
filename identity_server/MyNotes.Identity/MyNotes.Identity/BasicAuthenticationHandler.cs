using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace MyNotes.Identity;

public sealed class BasicAuthenticationOptions : AuthenticationSchemeOptions
{
    public string Login { get; set; }

    public string Password { get; set; }
}

public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
{
    public BasicAuthenticationHandler(IOptionsMonitor<BasicAuthenticationOptions> options, ILoggerFactory logger,
        UrlEncoder encoder, ISystemClock clock) : base(
        options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(AuthenticateResult.NoResult());
        if (AuthenticationHeaderValue.TryParse(Request.Headers["Authorization"], out var header))
        {
            var parameter = Encoding.UTF8.GetString(Convert.FromBase64String(header.Parameter));
            return Task.FromResult(parameter == $"{Options.Login}:{Options.Password}"
                ? AuthenticateResult.Success(CreateTicket())
                : AuthenticateResult.Fail("Incorrect header"));
        }

        return Task.FromResult(AuthenticateResult.Fail("Incorrect header"));
    }

    private AuthenticationTicket CreateTicket()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Admin")
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new GenericPrincipal(identity, null);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return ticket;
    }
}