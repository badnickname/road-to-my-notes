using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyNotes.Identity.Models;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace MyNotes.Identity.Controllers;

public sealed class AuthorizeController : Controller
{
    private readonly IOpenIddictApplicationManager _application;
    private readonly IOpenIddictAuthorizationManager _authorization;
    private readonly EmailService _emailService;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AuthorizeController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager,
        EmailService emailService, IOpenIddictApplicationManager application,
        IOpenIddictAuthorizationManager authorization)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _emailService = emailService;
        _application = application;
        _authorization = authorization;
    }

    /// <summary>
    ///     Форма аутентификации
    /// </summary>
    [HttpGet]
    [Route("~/connection/authorize")]
    public IActionResult GetAuthorizeForm()
    {
        return View("Index");
    }

    /// <summary>
    ///     Аутентифицироваться в приложении как пользователь
    /// </summary>
    /// <param name="userName">Имя</param>
    /// <param name="password">Пароль</param>
    [HttpPost]
    [Route("~/connection/authorize")]
    public async Task<IActionResult> AuthorizeUser(string userName, string password)
    {
        var user = await _userManager.FindByNameAsync(userName);

        if (user is null || !user.EmailConfirmed) return View("Index", new AuthorizeResult { HasError = true });

        var request = HttpContext.GetOpenIddictServerRequest()!;

        var application = await _application.FindByIdAsync(request.ClientId);

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

    /// <summary>
    ///     Создать новую учетную запись
    /// </summary>
    /// <param name="userName">Имя пользователя</param>
    /// <param name="password">Пароль</param>
    /// <param name="email">Email</param>
    [HttpPut]
    [Route("~/connection/authorize")]
    public async Task<IActionResult> RegisterUser(string userName, string password, string email)
    {
        var identity = new IdentityUser(userName)
        {
            Email = email
        };
        var result = await _userManager.CreateAsync(identity, password);
        if (!result.Succeeded) return StatusCode(500, result.Errors);

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(identity);
        _emailService.Send(email, "Accept", token);

        return Ok();
    }

    /// <summary>
    ///     Подтвердить Email для учетной записи
    /// </summary>
    /// <param name="userName">Имя пользователя</param>
    /// <param name="token">Сгенерированный токен</param>
    [HttpPatch]
    [Route("~/connection/authorize")]
    public async Task<IActionResult> ConfirmEmail(string userName, string token)
    {
        var identity = await _userManager.FindByNameAsync(userName);
        if (identity is null) return StatusCode(500);

        var result = await _userManager.ConfirmEmailAsync(identity, token);
        return result.Succeeded ? Ok() : StatusCode(500);
    }
}