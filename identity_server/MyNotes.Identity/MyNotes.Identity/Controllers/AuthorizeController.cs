using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyNotes.Identity.Models;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace MyNotes.Identity.Controllers;

public sealed class AuthorizeController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AuthorizeController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    [Route("~/connection/authorize")]
    public IActionResult GetAuthorizeForm()
    {
        return View("Index");
    }

    [HttpPost]
    [Route("~/connection/authorize")]
    public async Task<IActionResult> AuthorizeUser(string userName, string password)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user is null) return View("Index", new AuthorizeResult { HasError = true });

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded) return View("Index", new AuthorizeResult { HasError = true });

        var roles = (await _userManager.GetRolesAsync(user)).ToImmutableArray();

        var identity =
            new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, OpenIddictConstants.Claims.Name,
                OpenIddictConstants.Claims.Role);
        identity
            .SetClaim(OpenIddictConstants.Claims.Subject, user.Id)
            .SetClaim(OpenIddictConstants.Claims.Email, user.Email)
            .SetClaim(OpenIddictConstants.Claims.Name, user.UserName)
            .SetClaims(OpenIddictConstants.Claims.Role, roles);

        identity.SetDestinations(static claim => claim.Type switch
        {
            OpenIddictConstants.Claims.Name or OpenIddictConstants.Claims.Subject
                or OpenIddictConstants.Claims.Role => new[]
                    { OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken },
            _ => new[] { OpenIddictConstants.Destinations.AccessToken }
        });

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}