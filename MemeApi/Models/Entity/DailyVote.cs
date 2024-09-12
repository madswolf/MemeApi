using MemeApi.Models.DTO;
using System;

namespace MemeApi.Models.Entity;

public class DailyVote : DubloonEvent
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
