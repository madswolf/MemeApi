using System;
using MemeApi.Models.DTO.Challenges;
using MemeApi.Models.Entity.Dubloons;

namespace MemeApi.Models.Entity.Challenges;

public abstract record ChallengeAttempt
{
        public string Id { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public string ChallengeId { get; set; }
        
        public Challenge AttemptedChallenge { get; set; }
        
        public string OwnerId { get; set; }
        
        public User Owner { get; set; }
        
        public string? DubloonEventId { get; set; }
        
        public DubloonEvent? DubloonEvent { get; set; }
        
        public abstract ChallengeAttemptDTO ToChallengeAttemptDTO();
}