namespace Popug.Billing.Api.Consumers;

public class TaskCreatedData
{
    public int TaskId { get; set; }
    public string Description { get; set; } = null!;
    public string AssignTo { get; set; } = null!;
}