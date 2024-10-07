namespace MemeApi.Models.DTO.Dubloons;

/// <summary>
/// A DTO of user info
/// </summary>
/// <param name="Id"> Users Id </param>
/// <param name="UserName"> Users name </param>
/// <param name="Type"> The type of the Dubloon event </param>
/// <param name="Dubloons"> The type of the Dubloon event </param>
public record DubloonEventInfoDTO(string Id, string? UserName, string Type, int Dubloons);
