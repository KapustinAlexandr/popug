using Newtonsoft.Json;

namespace Popug.Analytic.Api.Consumers;

public class AuthEventDetailsData
{
    public required string Email { get; set; }

    [JsonProperty("first_name")]
    public required string FirstName { get; set; }

    [JsonProperty("last_name")]
    public required string LastName { get; set; }
}