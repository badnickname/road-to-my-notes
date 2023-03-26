using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyNotes.Share;
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
    [HttpPost]
    [Authorize(AuthenticationSchemes = Constants.AuthenticationScheme)]
    [Route("~/clients")]
    public async Task<IActionResult> RegisterClient([FromBody] ClientOptions dto)
    {
        if (await _applicationManager.FindByClientIdAsync(dto.ClientId) is null)
            await _applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = dto.ClientId,
                ClientSecret = dto.ClientSecret,
                DisplayName = dto.DisplayName,
                ConsentType = ConsentTypes.Explicit,
                Type = ClientTypes.Public,
                PostLogoutRedirectUris =
                {
                    new Uri(dto.RedirectUrl)
                },
                RedirectUris =
                {
                    new Uri(dto.RedirectUrl)
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