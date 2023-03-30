using System.Collections.Generic;
using System.Linq;
using MemeApi.Models.DTO;

namespace MemeApi.Models.Entity
{
    public class Votable
    {
        public int Id { get; set; }
        public List<Topic> Topics { get; set; }
        public List<Vote> Votes { get; set; }
    }
}
