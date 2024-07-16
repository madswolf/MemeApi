#nullable disable warnings
namespace MemeApi.Models.Entity;

public class Meme : Votable
{
    public MemeVisual MemeVisual { get; set; }
    public MemeText? TopText { get; set; }
    public MemeText? BottomText { get; set; }
}
