#nullable disable warnings
using System.Text.Json;
using MemeApi.Models.DTO.Memes;

namespace MemeApi.Models.Entity.Memes;

public class Meme : Votable
{
    public string VisualId { get; set; }
    public MemeVisual Visual { get; set; }

    public string? TopTextId { get; set; }
    public MemeText? TopText { get; set; }

    public string? BottomTextId { get; set; }
    public MemeText? BottomText { get; set; }
    public override VotableComponentDTO ToComponentDTO(string mediaHost)
    {
        var data = new
        {
            Visual = Visual.ToComponentDTO(mediaHost),
            TopText = TopText?.ToComponentDTO(mediaHost),
            BottomText = BottomText?.ToComponentDTO(mediaHost)
        };
        return new VotableComponentDTO(JsonSerializer.Serialize(data), Id, VoteAverage(), CreatedAt, Owner?.UserName ?? "No one");
    }
}
