using System.Collections.Generic;

namespace MemeApi.Models.Entity
{
    public class Votable
    {
        public int Id { get; set; }
        public Topic Topic { get; set; }
        public List<Vote> Votes { get; set; }
    }
}
