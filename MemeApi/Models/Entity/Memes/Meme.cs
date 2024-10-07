#nullable disable warnings
namespace MemeApi.Models.Entity.Memes;

public class Meme : Votable
{
    public string VisualId { get; set; }
    public MemeVisual Visual { get; set; }

    public string? TopTextId { get; set; }
    public MemeText? TopText { get; set; }

    public string? BottomTextId { get; set; }
    public MemeText? BottomText { get; set; }
}
