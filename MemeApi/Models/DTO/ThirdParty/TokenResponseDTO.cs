using Newtonsoft.Json;

namespace MemeApi.Models.DTO.ThirdParty;

public record TokenResponseDTO
{
    [JsonProperty("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    [JsonProperty("token_type")]
    public string TokenType { get; init; } = "Bearer";

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; init; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; init; } = string.Empty;
}
