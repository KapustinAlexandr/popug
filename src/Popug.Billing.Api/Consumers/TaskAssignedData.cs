namespace Popug.Billing.Api.Consumers;

public class TaskAssignedData
{
    public int TaskId { get; set; }

    public string AssignTo { get; set; } = null!;
}