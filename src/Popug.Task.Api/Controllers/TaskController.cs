using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Popug.Tasks.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TaskController : ControllerBase
{
    [Authorize]
    [HttpGet("whoami")]
    public IActionResult WhoAmI()
    {
        return new OkObjectResult(new
        {
            User.Identity?.Name,
            Roles = User.Claims.Where(w=>w.Type == ClaimsIdentity.DefaultRoleClaimType).Select(w=>w.Value).ToList()
        });
    }

    [Authorize(Roles = "admin")]
    [HttpGet("admin")]
    public IActionResult Admin()
    {
        return new OkObjectResult(User.Identity?.Name);
    }
}