using System;
using MemeApi.Models.DTO.Dubloons;
using MemeApi.Models.Entity.Memes;

namespace MemeApi.Models.Entity.Dubloons;

public record DailyVote : DubloonEvent
{
    public string VoteId { get; set; }
    public Vote Vote { get; set; }

    public override DubloonEventInfoDTO ToDubloonEventInfoDTO() => new DailyVoteDTO(
        Id,
        Owner.UserName,
        (int)Math.Floor(Dubloons),
        VoteId
    );
}
