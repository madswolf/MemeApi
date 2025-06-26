using System.Linq;
using MemeApi.Models.DTO.Challenges;
using MemeApi.Models.Entity.Challenges;

namespace MemeApi.library.Extensions;

public static class ChallengeExtensions
{
    public static ChallengeDTO ToChallengeDTO(this TriviaChallenge triviaChallenge)
    {
        return new ChallengeDTO
        {
            Category = ChallengeCategory.Trivia,
            Attempts = triviaChallenge.Attempts.Select(attempt => attempt.ToChallengeAttemptDTO());
        };
    }
    
    public static ChallengeAttemptDTO ToChallengeAttemptDTO(this TriviaAnswer triviaAnswer)
    {
        return new ChallengeDTO
        {
            Category = ChallengeCategory.Trivia,
            Attempts = triviaAnswer.Attempts.Select(ToChallengeAttemptDTO());
        };
    }
}