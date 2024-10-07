namespace MemeApi.Models.DTO.Dubloons;

/// <summary>
/// A DTO of user info
/// </summary>
/// <param name="Id"> Users Id </param>
/// <param name="UserName"> Users name </param>
/// <param name="Type"> The type of the Dubloon event </param>
/// <param name="Dubloons"> The type of the Dubloon event </param>
/// <param name="OtherUserName"> The username of the other party to the transaction. </param>
public record TransactionInfoDTO(string Id, string? UserName, int Dubloons, string? OtherUserName) :
    DubloonEventInfoDTO(Id, UserName, "Transaction", Dubloons);
