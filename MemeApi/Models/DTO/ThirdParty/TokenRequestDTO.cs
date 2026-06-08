namespace MemeApi.Models.DTO.ThirdParty;

// Property names are snake_case to match OAuth2 form field conventions.
// ASP.NET Core form binding matches case-insensitively on the full key name.
// Fields are individually required depending on grant_type; validation is done in the controller.
public class TokenRequestDTO
{
    public string grant_type { get; set; } = string.Empty;

    // Required for grant_type=discord_bot
    public string? client_secret { get; set; }
    public string? discord_user_id { get; set; }
    public string? discord_username { get; set; }

    // Required for grant_type=refresh_token
    public string? refresh_token { get; set; }
}
