using System;
using System.Collections.Generic;
using MemeApi.Models.DTO.Challenges;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MemeApi.Models.Entity.Challenges;

public abstract record Challenge
{
        public string Id { get; set; }
        public ChallengeCategory Category { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public List<ChallengeAttempt> Attempts { get; set; }
        public abstract ChallengeDTO ToChallengeDTO();
}