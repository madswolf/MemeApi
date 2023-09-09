namespace MemeApi.Models.Entity;

public class Meme : Votable
{
    public MemeVisual MemeVisual { get; set; }
    public MemeSound MemeSound { get; set; }
    public MemeText Toptext { get; set; }
    public MemeText BottomText { get; set; }
}
