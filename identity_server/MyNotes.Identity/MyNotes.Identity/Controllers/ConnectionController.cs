using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using SignInResult = Microsoft.AspNetCore.Mvc.SignInResult;

namespace MyNotes.Identity.Controllers;

public sealed class ConnectionController : Controller
{
    private readonly UserManager<IdentityUser> _manager;

    public ConnectionController(UserManager<IdentityUser> manager)
    {
        _manager = manager;
    }

    [HttpGet]
    [Route("~/connection/authorize")]
    public IActionResult GetAuthorizeForm()
    {
        return View("Authorize");
    }

    [HttpPost]
    [HttpGet]
    [Route("~/connection/token")]
    public async Task<IActionResult> GetToken()
    {
        var request = HttpContext.GetOpenIddictServerRequest()!;

        if (request.IsAuthorizationCodeGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var user = await _manager.FindByIdAsync(result.Principal.GetClaim(Claims.Subject));
            if (user is null)
                return new ForbidResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The token is no longer valid."
                    }));

            var identity = new ClaimsIdentity(result.Principal.Claims,
                TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);

            var roles = (await _manager.GetRolesAsync(user)).ToImmutableArray();

            identity
                .SetClaim(Claims.Subject, user.Id)
                .SetClaim(Claims.Email, user.Email)
                .SetClaim(Claims.Name, user.UserName)
                .SetClaims(Claims.Role, roles);

            identity.SetDestinations(static claim => claim.Type switch
            {
                Claims.Name or Claims.Subject or Claims.Role => new[]
                    { Destinations.AccessToken, Destinations.IdentityToken },
                _ => new[] { Destinations.AccessToken }
            });

            return new SignInResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }

        throw new InvalidOperationException("Grant Type не реализован");
    }
}