using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MyNotes.Identity.Areas.Default.Controllers;

public sealed class UserController : Controller
{
    private readonly EmailService _emailService;
    private readonly UserManager<IdentityUser> _userManager;

    public UserController(EmailService emailService, UserManager<IdentityUser> userManager)
    {
        _emailService = emailService;
        _userManager = userManager;
    }

    /// <summary>
    ///     Создать новую учетную запись
    /// </summary>
    /// <param name="userName">Имя пользователя</param>
    /// <param name="password">Пароль</param>
    /// <param name="email">Email</param>
    [HttpPut]
    [Route("~/users")]
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
    [HttpPost]
    [Route("~/users")]
    public async Task<IActionResult> ConfirmEmail(string userName, string token)
    {
        var identity = await _userManager.FindByNameAsync(userName);
        if (identity is null) return StatusCode(500);

        var result = await _userManager.ConfirmEmailAsync(identity, token);
        return result.Succeeded ? Ok() : StatusCode(500);
    }
}