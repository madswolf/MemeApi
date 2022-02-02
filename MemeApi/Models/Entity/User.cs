using System;
using System.Collections.Generic;

namespace MemeApi.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string ProfilePicFile { get; set; }
        public string PasswordHash { get; set; }
        public byte[] Salt { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<Vote> Votes { get; set; }
    }
}
