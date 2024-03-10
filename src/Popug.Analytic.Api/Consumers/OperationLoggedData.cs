namespace Popug.Analytic.Api.Consumers;

public class OperationLoggedData
{
    public string Type { get; set; } = null!;
    public int OperationId { get; set; }

    public int AccountId { get; set; }

    public string PopugPublicId { get; set; } = null!;
    public decimal Amount { get; set; }

    public DateTimeOffset OperationDate { get; set; }
}