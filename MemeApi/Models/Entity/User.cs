using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace MemeApi.Models.Entity
{
    public class User : IdentityUser<int>
    {
        public string ProfilePicFile { get; set; }
        public List<Vote> Votes { get; set; }
        public List<Topic>  Topics { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public DateTime LastLoginAt { get; set; }
    }
}

