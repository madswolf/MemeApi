using Newtonsoft.Json;

namespace MemeApi.Models.DTO.ThirdParty;

// Per RFC 7662 token introspection response.
public record IntrospectionResponseDTO
{
    [JsonProperty("active")]
    public bool Active { get; init; }

    [JsonProperty("sub")]
    public string? Sub { get; init; }

    [JsonProperty("username")]
    public string? Username { get; init; }

    [JsonProperty("exp")]
    public long? Exp { get; init; }

    [JsonProperty("iss")]
    public string? Iss { get; init; }

    [JsonProperty("aud")]
    public string? Aud { get; init; }
}
