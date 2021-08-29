namespace MemeApi.Models.Entity
{
    public class Meme : Votable
    {
        public MemeVisual MemeVisual { get; set; }
        public MemeSound MemeSound { get; set; }
        public MemeTopText MemeTopText { get; set; }
        public MemeBottomText MemeBottomtext { get; set; }
    }
}
