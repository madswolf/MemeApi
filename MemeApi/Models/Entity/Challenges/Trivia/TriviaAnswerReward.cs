using System;
using MemeApi.Models.DTO.Dubloons;
using MemeApi.Models.Entity.Dubloons;

namespace MemeApi.Models.Entity.Challenges.Trivia;

public record TriviaAnswerReward : DubloonEvent
{
    public string TriviaAnswerId { get; set; }
    public TriviaAnswer TriviaAnswer { get; set; }
    public override DubloonEventInfoDTO ToDubloonEventInfoDTO() => new(
        UserId,
        Owner.UserName,
        "TriviaAnswerReward",
        (int)Math.Floor(Dubloons)
    );
}