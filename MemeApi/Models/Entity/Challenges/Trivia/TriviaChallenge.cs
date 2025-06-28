using System.Collections.Generic;
using System.Linq;
using MemeApi.Models.DTO.Challenges;

namespace MemeApi.Models.Entity.Challenges.Trivia;

public record TriviaChallenge : Challenge
{
        public string Question { get; set; }
        public TriviaOption CorrectOption { get; set; }
        public List<TriviaOption> Options { get; set; }
        public override ChallengeDTO ToChallengeDTO()
        {
            List<TriviaOption> options = Options
                .Append(CorrectOption)
                .OrderBy(option => option.Index)
                .ToList();
            
            return new TriviaChallengeDTO()
            {
                Category = ChallengeCategory.Trivia,
                Attempts = Attempts.Select(attempt => attempt.ToChallengeAttemptDTO()).ToList(),
                Options = options
            };
        }
}