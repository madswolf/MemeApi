using MemeApi.Models.Entity;

namespace MemeApi.Models
{
    public class Vote
    {
        public int Id { get; set; }
        public bool Upvote { get; set; }
        public User User { get; set; }
        public Votable Element { get; set; }
    }
}
