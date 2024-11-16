#nullable disable warnings
using MemeApi;
using MemeApi.Models.DTO.Memes;

namespace MemeApi.Models.Entity.Memes;

public class MemeText : Votable
{
    public string Text { get; set; }
    public MemeTextPosition Position { get; set; }
    public override VotableComponentDTO ToComponentDTO(string _)
    {
        return new VotableComponentDTO(Text, Id, VoteAverage(), CreatedAt, Owner?.UserName ?? "No one");
    }
}
