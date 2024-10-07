namespace MemeApi.Models.DTO.Dubloons;
/// <summary>
/// A DTO of user info
/// </summary>
/// <param name="Id"> Users Id </param>
/// <param name="UserName"> Users name </param>
/// <param name="Type"> The type of the Dubloon event </param>
/// <param name="Dubloons"> The type of the Dubloon event </param>
/// <param name="VoteId"> The Id of the vote that spawned the event </param>
public record DailyVoteDTO(string Id, string? UserName, int Dubloons, string VoteId) :
    DubloonEventInfoDTO(Id, UserName, "DailyVote", Dubloons);
