using System.Collections.Generic;

namespace MemeApi.Models.Entity.Challenges;

public abstract class Challenge
{
        public string Id { get; set; }
        
        public string Solution { get; set; }
        
        public List<ChallengeAttempt> Attempts { get; set; }
}