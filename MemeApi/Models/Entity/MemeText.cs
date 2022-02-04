using MemeApi.Models.Entity;
using System.Collections.Generic;

namespace MemeApi.Models
{
    public class MemeText : Votable
    {
        public string Memetext { get; set; }
        public MemeTextPosition postion { get; set; } 
    }
}
