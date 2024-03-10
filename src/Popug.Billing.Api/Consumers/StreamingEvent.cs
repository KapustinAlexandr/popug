namespace Popug.Billing.Api.Consumers;

public class StreamingEvent<T> where T : class
{
    /// <summary>
    /// Тип события: created|updated|deleted
    /// </summary>
    public required string Type { get; set; }
    /// <summary>
    /// Изменившаяся сущность
    /// </summary>
    public required T Data { get; set; }
}