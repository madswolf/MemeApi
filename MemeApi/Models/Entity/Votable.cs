﻿using System.Collections.Generic;

namespace MemeApi.Models
{
    public class Votable
    {
        public long Id { get; set; }
        public List<Vote> Votes { get; set; }
    }
}