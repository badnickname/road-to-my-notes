using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace MyNotes.Identity.Areas.Default.Controllers;

public sealed class ClientController : Controller
{
    private readonly IOpenIddictApplicationManager _applicationManager;

    public ClientController(IOpenIddictApplicationManager applicationManager)
    {
        _applicationManager = applicationManager;
    }

    /// <summary>
    ///     Добавить новое клиентское приложение
    /// </summary>
    /// <param name="clientId">Идентификатор приложения</param>
    /// <param name="displayName">Название</param>
    /// <param name="redirectUrl">Ссылка на приложение</param>
    [HttpPost]
    [Authorize(AuthenticationSchemes = "Basic")]
    [Route("~/clients")]
    public async Task<IActionResult> RegisterClient(string clientId, string displayName, string redirectUrl)
    {
        if (await _applicationManager.FindByClientIdAsync(clientId) is null)
            await _applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = clientId,
                ConsentType = ConsentTypes.Explicit,
                Type = ClientTypes.Public,
                PostLogoutRedirectUris =
                {
                    new Uri(redirectUrl)
                },
                RedirectUris =
                {
                    new Uri(redirectUrl)
                },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles
                },
                Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange
                }
            });

        return Ok();
    }
}