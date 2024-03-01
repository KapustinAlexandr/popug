using Popug.Tasks.Api.Data;

namespace Popug.Tasks.Api.Logic;

public class TaskLogic
{
    private readonly TasksDb _db;

    public TaskLogic(TasksDb db)
    {
        _db = db;
    }
}