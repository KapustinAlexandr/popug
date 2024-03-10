namespace Popug.Billing.Api.Consumers;

public class TaskCompletedData
{
    public int TaskId { get; set; }

    public string AssignTo { get; set; } = null!;
}