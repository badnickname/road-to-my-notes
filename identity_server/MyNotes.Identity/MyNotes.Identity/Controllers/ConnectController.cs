using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace MyNotes.Identity.Controllers;

public sealed class ConnectController : Controller
{
    private readonly IOpenIddictApplicationManager _manager;

    public ConnectController(IOpenIddictApplicationManager manager)
    {
        _manager = manager;
    }

    [HttpPost("~/connect/token")]
    [HttpGet("~/connect/token")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()!;
        if (!request.IsClientCredentialsGrantType()) throw new NotImplementedException("Grant Type не реализован");

        var application = await _manager.FindByClientIdAsync(request.ClientId);
        if (application is null) throw new InvalidOperationException("Client не зарегистирован");

        var identity = new ClaimsIdentity(
            TokenValidationParameters.DefaultAuthenticationType,
            Claims.Name,
            Claims.Role);

        identity.SetClaim(Claims.Subject, await _manager.GetClientIdAsync(application));
        identity.SetClaim(Claims.Name, await _manager.GetDisplayNameAsync(application));
        identity.SetDestinations(static claim => claim.Type switch
        {
            Claims.Name or Claims.Subject when claim.Subject!.HasScope(Scopes.Profile)
                => new[] { Destinations.AccessToken, Destinations.IdentityToken },
            _ => new[] { Destinations.AccessToken }
        });

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}