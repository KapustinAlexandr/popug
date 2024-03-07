namespace Popug.Tasks.Api.Logic;

public class AuthEventData
{
    /// <summary>
    /// REGISTER
    /// </summary>
    public required string Type { get; set; } 

    public required string UserId { get; set; } 

    public required AuthEventDetailsData Details { get; set; } 

}