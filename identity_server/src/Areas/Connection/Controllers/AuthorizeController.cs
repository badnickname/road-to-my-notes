using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyNotes.Identity.Areas.Connection.Models;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace MyNotes.Identity.Areas.Connection.Controllers;

public sealed class AuthorizeController : Controller
{
    private readonly IOpenIddictApplicationManager _application;
    private readonly IOpenIddictAuthorizationManager _authorization;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AuthorizeController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager,
        IOpenIddictApplicationManager application,
        IOpenIddictAuthorizationManager authorization)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _application = application;
        _authorization = authorization;
    }

    /// <summary>
    ///     Форма аутентификации
    /// </summary>
    [HttpGet]
    [Route("~/connect/authorize")]
    [Area("Connection")]
    public IActionResult GetAuthorizeForm()
    {
        return View("Index", new AuthorizeResult
        {
            HasError = false
        });
    }

    /// <summary>
    ///     Аутентифицироваться в приложении как пользователь
    /// </summary>
    /// <param name="userName">Имя</param>
    /// <param name="password">Пароль</param>
    [HttpPost]
    [Route("~/connect/authorize")]
    [Area("Connection")]
    public async Task<IActionResult> AuthorizeUser()
    {
        var userName = HttpContext.Request.Form["userName"];
        var password = HttpContext.Request.Form["password"];
        
        var user = await _userManager.FindByNameAsync(userName);

        if (user is null || !user.EmailConfirmed) return View("Index", new AuthorizeResult { HasError = true });

        var request = HttpContext.GetOpenIddictServerRequest()!;

        var application = await _application.FindByClientIdAsync(request.ClientId);

        var clientId = await _application.GetIdAsync(application);

        object? authorization = null;
        await foreach (var auth in _authorization.FindAsync(user.Id, clientId, Statuses.Valid,
                           AuthorizationTypes.Permanent)) authorization = auth;

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded) return View("Index", new AuthorizeResult { HasError = true });

        var roles = (await _userManager.GetRolesAsync(user)).ToImmutableArray();

        var identity =
            new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name,
                Claims.Role);
        identity
            .SetClaim(Claims.Subject, user.Id)
            .SetClaim(Claims.Email, user.Email)
            .SetClaim(Claims.Name, user.UserName)
            .SetClaims(Claims.Role, roles);

        authorization ??= await _authorization.CreateAsync(identity, user.Id, clientId, AuthorizationTypes.Permanent,
            identity.GetScopes());

        identity.SetAuthorizationId(await _authorization.GetIdAsync(authorization));

        identity.SetDestinations(static claim => claim.Type switch
        {
            Claims.Name or Claims.Subject
                or Claims.Role => new[]
                    { Destinations.AccessToken, Destinations.IdentityToken },
            _ => new[] { Destinations.AccessToken }
        });

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}