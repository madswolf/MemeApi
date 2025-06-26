using System.Collections.Generic;
using MemeApi.Models.DTO.Challenges;

namespace MemeApi.Models.Entity.Challenges;

public abstract record Challenge
{
        public string Id { get; set; }
        public ChallengeCategory Category { get; set; }
        public List<ChallengeAttempt> Attempts { get; set; }
        public abstract ChallengeDTO ToChallengeDTO();
}