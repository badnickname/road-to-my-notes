using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyNotes.Share;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace MyNotes.Identity.Areas.Default.Controllers;

public sealed class ClientController : Controller
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictScopeManager _scopeManager;

    public ClientController(IOpenIddictApplicationManager applicationManager, IOpenIddictScopeManager scopeManager)
    {
        _applicationManager = applicationManager;
        _scopeManager = scopeManager;
    }

    /// <summary>
    ///     Добавить новое клиентское приложение
    /// </summary>
    [HttpPost]
    [Authorize(AuthenticationSchemes = Constants.AuthenticationScheme)]
    [Route("~/clients")]
    public async Task<IActionResult> RegisterClient([FromBody] ClientOptions dto)
    {
        if (await _applicationManager.FindByClientIdAsync(dto.ClientId) is not null) return Ok();

        if (dto.Type == ClientOptions.ClientType.Client)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = dto.ClientId,
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
            };

            foreach (var scope in dto.Scopes) descriptor.Permissions.Add(Permissions.Prefixes.Scope + scope);

            await _applicationManager.CreateAsync(descriptor);
        }
        else
        {
            await _applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                DisplayName = dto.DisplayName,
                ClientId = dto.ClientId,
                ClientSecret = dto.ClientSecret,
                Permissions =
                {
                    Permissions.Endpoints.Introspection
                }
            });

            foreach (var scope in dto.Scopes)
            {
                var descriptor = new OpenIddictScopeDescriptor
                {
                    Name = scope,
                    Resources =
                    {
                        dto.ClientId
                    }
                };

                var createdScope = await _scopeManager.FindByNameAsync(scope);
                if (createdScope is not null)
                    await _scopeManager.UpdateAsync(scope, descriptor);
                else
                    await _applicationManager.CreateAsync(descriptor);
            }
        }

        return Ok();
    }
}