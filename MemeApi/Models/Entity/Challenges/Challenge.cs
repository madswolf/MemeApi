using System.Collections.Generic;

namespace MemeApi.Models.Entity.Challenges;

public abstract record Challenge
{
        public string Id { get; set; }
        public ChallengeCategory Category { get; set; }
        public List<ChallengeAttempt> Attempts { get; set; }
}