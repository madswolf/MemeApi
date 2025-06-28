using System.Collections.Generic;
using MemeApi.Models.Entity.Challenges;
using MemeApi.Models.Entity.Challenges.Trivia;

namespace MemeApi.Models.DTO.Challenges;

public record TriviaChallengeDTO : ChallengeDTO
{
        public string Question { get; init; }
        
        public List<TriviaOption> Options { get; init; } 
}