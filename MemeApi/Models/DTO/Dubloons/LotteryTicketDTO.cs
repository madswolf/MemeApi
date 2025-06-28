namespace MemeApi.Models.DTO.Dubloons;
/// <summary>
/// A DTO of user info
/// </summary>
/// <param name="Id"> Users Id </param>
/// <param name="UserName"> Users name </param>
/// <param name="Type"> The type of the Dubloon event </param>
/// <param name="Dubloons"> The type of the Dubloon event </param>
/// <param name="ItemId"> The Id of the Item that was won </param>
public record LotteryTicketDTO(string Id, string? UserName, int Dubloons, string ItemId) :
    DubloonEventInfoDTO(Id, UserName, "LotteryTicket", Dubloons);