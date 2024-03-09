using Core;
using LinqToDB;
using LinqToDB.Common;
using Mxm.Kafka;
using Popug.Tasks.Api.Common;
using Popug.Tasks.Api.Data;

namespace Popug.Tasks.Api.Logic;

public class TaskLogic
{
    private readonly TasksDb _db;
    private readonly KafkaProducer _producer;

    [Sql.Expression("random()", ServerSideOnly = true)]
    public static int Random()
    {
        throw new InvalidOperationException();
    }

    public TaskLogic(TasksDb db, KafkaProducer producer)
    {
        _db = db;
        _producer = producer;
    }

    public IQueryable<PopugEntity> GetPopugsValidForAssign()
    {
        return _db.Popugs.Where(w => !new[] { "admin", "manager" }.Contains(w.UserRole));
    }

    public PopugEntity GetRandom(List<PopugEntity> source)
    {
        return source[System.Random.Shared.Next(0, source.Count - 1)];
    }
    
    public async Task<TaskEntity> CreateTask(string userId, string description)
    {
        // Получим одного рандомного валидного попуга из БД
        var popug = await GetPopugsValidForAssign()
            .OrderBy(k => Random())
            .FirstOrDefaultAsync();

        if (popug == null)
        {
            throw new Exception("FATAL. No popugs yet.");
        }

        using var tran = TransactionHelper.GetTransaction();

        var newTaskId = await _db.InsertWithInt32IdentityAsync(new TaskEntity
        {
            CreatedBy = userId,
            AssignTo = popug.UserId,
            Description = description,
            IsDone = false
        });

        var newTask = await _db.Tasks.SingleAsync(w => w.TaskId == newTaskId);
        
        await _producer.SendEvent(
            KafkaTopic.TaskTracker.TaskStreaming, 
            new { newTask.TaskId, newTask.Description, newTask.AssignTo }.CUDCreated(), 
            version: 1, 
            key: newTask.TaskId);

        tran.Complete();

        return newTask;
    }
    
    public async Task CompleteTask(string userId, int taskId)
    {
        using (var tran = TransactionHelper.GetTransaction())
        {
            var rowsAffected = await _db.Tasks
                .Where(w => w.TaskId == taskId & !w.IsDone & w.AssignTo == userId)
                .Set(s => s.IsDone, () => true)
                .UpdateAsync();

            if (rowsAffected == 0)
            {
                return;
            }

            var task = await _db.Tasks.SingleAsync(w => w.TaskId == taskId);

            await _producer.SendEvent(KafkaTopic.TaskTracker.TaskCompleted, new { task.TaskId, task.AssignTo }, version: 1, key: task.TaskId);
            
            tran.Complete();
        }
    }
    
    public async Task ShuffleTasks()
    {
        var tasksQuery = _db.Tasks.Where(w => !w.IsDone).ToListAsync(); // Таски
        var popugsQuery = GetPopugsValidForAssign().ToListAsync(); // Кэш попугов из БД.

        await Task.WhenAll(tasksQuery, popugsQuery);

        var tasks = tasksQuery.Result; var popugs = popugsQuery.Result;

        if (tasks.IsNullOrEmpty())
            return;

        if (popugs == null)
            throw new Exception("FATAL. No popugs yet.");

        foreach (var task in tasks)
        {
            using (var tran = TransactionHelper.GetTransaction())
            {
                // Считаем что кроме нас никто не работает в системе, не защищаемся от возможных race conditions.
                await _db.Tasks
                    .Where(w => w.TaskId == task.TaskId)
                    .Set(s => s.AssignTo, () => GetRandom(popugs).UserId)
                    .UpdateAsync();
                
                await _producer.SendEvent(KafkaTopic.TaskTracker.TaskAssigned, new { task.TaskId, task.AssignTo }, version: 1, key: task.TaskId);

                tran.Complete();
            }
        }
    }

    public Task<List<TaskEntity>> GetActiveTasks(string userId)
    {
        return _db.Tasks.Where(w => w.AssignTo == userId & !w.IsDone).ToListAsync();
    }
}