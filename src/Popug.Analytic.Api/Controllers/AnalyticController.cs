using System.Globalization;
using LinqToDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Popug.Analytic.Api.Data;

namespace Popug.Analytic.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "admin")]
public class AnalyticController : ControllerBase
{

    private readonly AnalyticDb _db;

    public AnalyticController(AnalyticDb db)
    {
        _db = db;
    }



    [HttpGet("earned")]
    public async Task<IActionResult> EarnedToday()
    {
        return new OkObjectResult(
        (await _db.Operations
            .Where(w => w.OperationDate > DateTimeOffset.Now.Date)
            .Select(s => Sql.Ext.Sum(s.Credit).ToValue() - Sql.Ext.Sum(s.Debt).ToValue())
            .ToListAsync())
        .FirstOrDefault(0));
    }

    [HttpGet("popug_credit")]
    public async Task<IActionResult> PopugCredits()
    {
        var popugUnderZero = await
            (
                from o in _db.Operations
                join u in _db.Users on o.PublicUserId equals u.UserId
                group o by new {o.PublicUserId, u.UserName}
                into g
                select new
                {
                    UserId = g.Key.PublicUserId,
                    UserName = g.Key.UserName,
                    Balance = g.Sum(s => s.Debt) - g.Sum(s => s.Credit)
                }
            )
            .Where(w => w.Balance < 0)
            .ToListAsync();

        return new OkObjectResult(popugUnderZero);
    }

    [HttpGet("task_rating")]
    public async Task<IActionResult> TaskRating()
    {
        var myCal = CultureInfo.InvariantCulture.Calendar;

        var maxTasks = (await (
                from o in _db.Operations
                where o.Debt > 0 // Закрыли и выплатили попугу
                group o by new { o.OperationDate.Year, o.OperationDate.Month, o.OperationDate.Day }
                into g
                select new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Day,
                    MaxCost = g.Max(s => s.Debt)
                }
            )
            .ToListAsync())
            .Select(s=>new
            {
                Date = new DateTime(s.Year, s.Month, s.Day),
                WeekNumber = myCal.GetWeekOfYear(new DateTime(s.Year, s.Month, s.Day), CalendarWeekRule.FirstDay, DayOfWeek.Monday),
                s.Year,
                s.Month,
                s.Day,
                s.MaxCost
            })
            .OrderByDescending(o=>o.Date)
            .ToList();
        

        return new OkObjectResult(maxTasks);
    }
}