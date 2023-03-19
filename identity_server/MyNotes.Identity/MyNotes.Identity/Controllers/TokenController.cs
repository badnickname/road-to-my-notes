using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace MyNotes.Identity.Controllers;

public sealed class TokenController : Controller
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public TokenController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
        IOpenIddictApplicationManager applicationManager, IOpenIddictAuthorizationManager authorizationManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
    }

    /// <summary>
    ///     Authorization code flow
    /// </summary>
    [HttpPost]
    [HttpGet]
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [Route("~/connection/token")]
    public async Task<IActionResult> ExchangeAuthorizationToken()
    {
        var request = HttpContext.GetOpenIddictServerRequest()!;

        if (request.IsAuthorizationCodeGrantType())
        {
            var user = await _userManager.FindByIdAsync(HttpContext.User.GetClaim(OpenIddictConstants.Claims.Subject));
            if (user is null)
                return new ForbidResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The token is no longer valid."
                    }));

            var identity = new ClaimsIdentity(HttpContext.User.Claims,
                TokenValidationParameters.DefaultAuthenticationType, OpenIddictConstants.Claims.Name,
                OpenIddictConstants.Claims.Role);

            var roles = (await _userManager.GetRolesAsync(user)).ToImmutableArray();

            identity
                .SetClaim(OpenIddictConstants.Claims.Subject, user.Id)
                .SetClaim(OpenIddictConstants.Claims.Email, user.Email)
                .SetClaim(OpenIddictConstants.Claims.Name, user.UserName)
                .SetClaims(OpenIddictConstants.Claims.Role, roles);

            identity.SetDestinations(static claim => claim.Type switch
            {
                OpenIddictConstants.Claims.Name or OpenIddictConstants.Claims.Subject
                    or OpenIddictConstants.Claims.Role => new[]
                    {
                        OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken
                    },
                _ => new[] { OpenIddictConstants.Destinations.AccessToken }
            });

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens
            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("Grant Type не реализован");
    }
}