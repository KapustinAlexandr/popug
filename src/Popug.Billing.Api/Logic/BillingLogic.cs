using Core;
using LinqToDB;
using Mxm.Kafka;
using Popug.Billing.Api.Common;
using Popug.Billing.Api.Data;
using Popug.Tasks.Api.Common;

namespace Popug.Billing.Api.Logic;

public class BillingLogic
{

    private readonly BillingDb _db;
    private readonly KafkaProducer _producer;

    public BillingLogic(BillingDb db, KafkaProducer producer)
    {
        _db = db;
        _producer = producer;
    }

    private async Task<int> GetActiveBillingCycle(int accountId)
    {
        var exists =
            await _db.BillingCycles
                .FirstOrDefaultAsync(w => w.AccountId == accountId && w.CloseAt > DateTimeOffset.UtcNow);

        if (exists != null)
            return exists.BillingCycleId;

        var newCycleId = await _db.InsertWithInt32IdentityAsync(new BillingCycleEntity
        {
            AccountId = accountId,
            CloseAt = new DateTimeOffset(DateTime.Now.Date.Add(new TimeSpan(23, 59, 59)))
        });

        return newCycleId;
    }

    private async Task<int> GetAccountIdForUserId(string userId)
    {
        var exists = await _db.Accounts.FirstOrDefaultAsync(w => w.UserId == userId);
        if (exists != null)
        {
            return exists.AccountId;
        }

        var newAccountId = await _db.InsertWithInt32IdentityAsync(new AccountEntity
        {
            UserId = userId,
            Balance = 0
        });

        return newAccountId;
    }


    public decimal GetAssignCharge()
    {
        return decimal.Round(new decimal(Random.Shared.NextDouble() * 10) + 10, 2, MidpointRounding.AwayFromZero);
    }

    public decimal GetCompleteCharge()
    {
        return decimal.Round(new decimal(Random.Shared.NextDouble() * 20) + 20, 2, MidpointRounding.AwayFromZero);
    }

    public async Task WriteOffAssignedTask(int taskId, string assignTo)
    {
        var task = await _db.Tasks.SingleAsync(w => w.TaskId == taskId);
        var accountId = await GetAccountIdForUserId(assignTo);
        var cycleId = await GetActiveBillingCycle(accountId);


        using (var tran = TransactionHelper.GetTransaction())
        {
            var operationAmount = task.AssignCharge;
            var operationDate = DateTimeOffset.Now;

            var newOperationLogId = await _db.InsertWithInt32IdentityAsync(new OperationsLogEntity
            {
                AccountId = accountId,
                BillingCycleId = cycleId,
                Credit = operationAmount,
                Debt = 0,
                Description = $"Write-off for assigned task: [{task.TaskId}] {task.Description}",
                OperationDate = operationDate,
                ReasonId = task.TaskId.ToString(),
                ReasonType = "task"
            });

            await _db.Accounts
                .Where(w => w.AccountId == accountId)
                    .Set(s => s.Balance, item => item.Balance - operationAmount)
                .UpdateAsync();
            
            await _producer.SendEvent(
                KafkaTopic.Billing.OperationLogged,
                new
                {
                    Type = "write-off",
                    OperationId = newOperationLogId,
                    AccountId = accountId,
                    PopugPublicId = assignTo,
                    Amount = operationAmount,
                    OperationDate = operationDate
                },
                version: 1, key: newOperationLogId);

            tran.Complete();
        }
    }

    public async Task AccrualForCompletedTask(int taskId, string completedBy)
    {
        var task = await _db.Tasks.SingleAsync(w => w.TaskId == taskId);
        var accountId = await GetAccountIdForUserId(completedBy);
        var cycleId = await GetActiveBillingCycle(accountId);


        using (var tran = TransactionHelper.GetTransaction())
        {
            var operationAmount = task.CompleteCharge;
            var operationDate = DateTimeOffset.Now;

            var newOperationLogId = await _db.InsertWithInt32IdentityAsync(new OperationsLogEntity
            {
                AccountId = accountId,
                BillingCycleId = cycleId,
                Credit = 0,
                Debt = operationAmount,
                Description = $"Accrual for completed task: [{task.TaskId}] {task.Description}",
                OperationDate = operationDate,
                ReasonId = task.TaskId.ToString(),
                ReasonType = "task"
            });

            await _db.Accounts
                .Where(w => w.AccountId == accountId)
                    .Set(s => s.Balance, item => item.Balance + operationAmount)
                .UpdateAsync();

            await _producer.SendEvent(
                KafkaTopic.Billing.OperationLogged,
                new
                {
                    Type = "accrual",
                    OperationId = newOperationLogId,
                    AccountId = accountId,
                    PopugPublicId = completedBy,
                    Amount = operationAmount,
                    OperationDate = operationDate
                },
                version: 1, key: newOperationLogId);

            tran.Complete();
        }
    }

    public async Task ProcessClosedBillingCycles()
    {
        // Список не закрытых расчетных циклов, по порядку создания.
        var cycles = await (
                from bc in _db.BillingCycles
                join ac in _db.Accounts on bc.AccountId equals ac.AccountId
                where 
                    bc.PaymentOperationLogId == null & 
                    bc.CloseAt < DateTimeOffset.Now
                select new
                {
                    bc.AccountId,
                    bc.BillingCycleId,
                    bc.CloseAt,
                    PopugPublicId = ac.UserId
                }).OrderBy(o => o.CloseAt)
            .ToListAsync();

        foreach (var cycle in cycles)
        {
            // Посчитаем баланс на конец цикла
            var balance = await
            (
                from ol in _db.OperationsLogs
                join bc in _db.BillingCycles on ol.BillingCycleId equals bc.BillingCycleId
                where ol.AccountId == cycle.AccountId
                      // берём операции которые попали в наш цикл или в более ранний
                      & (bc.CloseAt < cycle.CloseAt | ol.BillingCycleId == cycle.BillingCycleId)
                group ol by ol.AccountId
                into grouped
                select new
                {
                    AccountId = grouped.Key,
                    Amount = grouped.Sum(s => s.Debt) - grouped.Sum(s => s.Credit)
                }
            ).SingleAsync();

            var newLog = new OperationsLogEntity
            {
                AccountId = cycle.AccountId,
                BillingCycleId = cycle.BillingCycleId,
                OperationDate = DateTimeOffset.Now,
                Debt = 0,
                ReasonType = "payment",
                ReasonId = "0"
            };

            if (balance.Amount > 0)
            {
                newLog.Credit = balance.Amount;
                newLog.Description = "Выплата средств в конце дня";
            }
            else
            {
                newLog.Credit = 0;
                newLog.Description = $"Для выплаты не достаточно средств. Баланс {balance.Amount} у.е.";
            }

            using (var tran = TransactionHelper.GetTransaction())
            {
                // Запись в лог
                var newOperationLogId = await _db.InsertWithInt32IdentityAsync(newLog);
                
                // Запись ссылки на операцию в цикл
                await _db.BillingCycles
                    .Where(w => w.BillingCycleId == cycle.BillingCycleId)
                    .Set(s => s.PaymentOperationLogId, () => newOperationLogId)
                    .UpdateAsync();
                
                await _producer.SendEvent(
                    KafkaTopic.Billing.OperationLogged,
                    new
                    {
                        Type = "payment",
                        OperationId = newOperationLogId,
                        AccoutnId = newLog.AccountId,
                        cycle.PopugPublicId,
                        balance.Amount,
                        newLog.OperationDate
                    },
                    version: 1, key: newOperationLogId);

                tran.Complete();
            }
        }
    }
    

}