namespace Popug.Tasks.Api.Common;

public class CUDEvent<T> where T : class
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