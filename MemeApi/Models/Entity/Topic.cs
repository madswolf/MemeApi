using System;
using System.Collections.Generic;

namespace MemeApi.Models.Entity
{
    public class Topic
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public User Owner { get; set; }
        public List<User> Moderators { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
