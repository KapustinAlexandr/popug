using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Popug.Task.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TaskController : ControllerBase
{
    [Authorize]
    [HttpGet("whoami")]
    public IActionResult WhoAmI()
    {
        return new OkObjectResult(User.Identity?.Name);
    }

    [Authorize(Roles = "admin")]
    [HttpGet("admin")]
    public IActionResult Admin()
    {
        return new OkObjectResult(User.Identity?.Name);
    }
}