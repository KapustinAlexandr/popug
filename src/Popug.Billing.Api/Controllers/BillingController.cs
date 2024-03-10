using LinqToDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Popug.Billing.Api.Data;
using Popug.Billing.Api.Logic;

namespace Popug.Billing.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly BillingDb _billingDb;
    private readonly BillingLogic _billingLogic;

    public BillingController(BillingDb billingDb, BillingLogic billingLogic)
    {
        _billingDb = billingDb;
        _billingLogic = billingLogic;
    }

    private string GetCurrentUserId()
    {
        return User.Claims
            .First(w => w.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
            .Value;
    }


    [HttpGet("account")]
    // Доступно всем    
    public async Task<IActionResult> GetUserInfo()
    {
        var account = await _billingDb.Accounts.FirstOrDefaultAsync(w => w.UserId == GetCurrentUserId());

        if (account == null)
        {
            return new OkResult();
        }

        return new OkObjectResult(new
        {
            account.Balance,
            Operations = await _billingDb.OperationsLogs
                .Where(w => w.AccountId == account.AccountId)
                .OrderByDescending(o => o.OperationDate)
                .ToListAsync()
        });
    }

    [HttpGet("earns")]
    [Authorize(Roles = "admin,buh")]
    public async Task<IActionResult> GetAllInfo()
    {
        var src = await (from ol in _billingDb.OperationsLogs
                where ol.ReasonType == "task"
                group ol by new { ol.OperationDate.Year, ol.OperationDate.Month, ol.OperationDate.Day }
                into g
                select new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Day,
                    Amount = g.Sum(s => s.Credit) - g.Sum(s => s.Debt)
                })
            .OrderByDescending(o => o.Year)
            .ThenByDescending(o => o.Month)
            .ThenByDescending(o => o.Day)
            .ToListAsync();
        
        var first = src.FirstOrDefault();

        if (first == null)
        {
            return new OkResult();
        }

        var firstDate = new DateTime(first.Year, first.Month, first.Day);
        if (firstDate != DateTime.Today)
        {
            return new OkObjectResult(new
            {
                TodayEarned = 0m,
                Statisitcs = src
            });
        }

        src.Remove(first);

        return new OkObjectResult(new
        {
            TodayEarned = first.Amount,
            Statisitcs = src
        });
    }
}