﻿using System;

namespace MemeApi.Models.DTO
{
    public class VoteDTO
    {
        public string Id { get; set; }
        public string VotableId { get; set; }
        public Upvote Upvote { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt {  get; set; }
        public DateTime LastUpdateAt { get; set; }
    }
}
