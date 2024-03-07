namespace Popug.Tasks.Api.Logic;

public class AuthAdminData
{
    /// <summary>
    /// GROUP_MEMBERSHIP
    /// </summary>
    public required string ResourceType { get; set; }

    /// <summary>
    /// CREATE, DELETE
    /// </summary>
    public required string OperationType { get; set; }

    /// <summary>
    /// users/{user_id_guid}/groups/{group_id_guid}
    /// </summary>
    public required string ResourcePath { get; set;}

    public required string Representation { get; set;}
}