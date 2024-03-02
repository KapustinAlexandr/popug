using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Popug.Tasks.Api.Common;
using Popug.Tasks.Api.Logic;

namespace Popug.Tasks.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly TaskLogic _taskLogic;

    public TaskController(TaskLogic taskLogic)
    {
        _taskLogic = taskLogic;
    }

    private string GetCurrentUserId()
    {
        return User.Claims
            .First(w => w.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
            .Value;
    }

    [HttpGet("whoami")]
    public IActionResult WhoAmI()
    {
        return new OkObjectResult(new
        {
            Id = GetCurrentUserId(),
            User.Identity?.Name,
            Roles = User.Claims.Where(w=>w.Type == ClaimsIdentity.DefaultRoleClaimType).Select(w=>w.Value).ToList()
        });
    }
    
    /// <summary>
    /// Создать задачу.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTask(CreateTaskRequest request)
    {
        var result = await _taskLogic.CreateTask(GetCurrentUserId(), request.Description);

        return new OkObjectResult(result);
    }

    /// <summary>
    /// Список активных задач.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTasks()
    {
        var result = await _taskLogic.GetActiveTasks(GetCurrentUserId());

        return new OkObjectResult(result);
    }

    /// <summary>
    /// Выполнить задачу.
    /// </summary>
    [HttpPost("{taskId}/complete")]
    public async Task<IActionResult> CompleteTask(int taskId)
    {
        await _taskLogic.CompleteTask(GetCurrentUserId(), taskId);
        return new OkResult();
    }

    /// <summary>
    /// Переназначить задачи.
    /// </summary>
    [Authorize(Roles = "admin,manager")]
    [HttpPost("shuffle")]
    public async Task<IActionResult> ShuffleTasks()
    {
        await _taskLogic.ShuffleTasks();
        return new OkResult();
    }
}