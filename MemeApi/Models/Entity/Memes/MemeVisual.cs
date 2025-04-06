#nullable disable warnings
using MemeApi.Models.DTO.Memes;

namespace MemeApi.Models.Entity.Memes;

public class MemeVisual : Votable
{
    public string Filename { get; set; }

    public override VotableComponentDTO ToComponentDTO( string mediaHost)
    {
        return new VotableComponentDTO(mediaHost + "visual/" + Filename, Id, VoteAverage(), CreatedAt, Owner?.UserName ?? "No one");
    }
}
