namespace MemeApi.Models.DTO.ThirdParty;

public class InitiateAuthResponseDTO
{
    public string temporary_password { get; set; } = string.Empty;
    public int expires_in { get; set; }
}
