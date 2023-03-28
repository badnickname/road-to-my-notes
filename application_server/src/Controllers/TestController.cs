using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyNotes.Application.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public string Get()
    {
        var user = HttpContext.User;
        return user.Identity?.Name ?? "empty";
    }
}