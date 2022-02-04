namespace MemeApi.Models.Entity
{
    public class MemeText : Votable
    {
        public string Memetext { get; set; }
        public MemeTextPosition postion { get; set; } 
    }
}
