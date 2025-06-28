using MemeApi.Models.DTO.Challenges;

namespace MemeApi.Models.Entity.Challenges.Trivia;

public record TriviaAnswer : ChallengeAttempt
{
        public int AnswerOptionIndex { get; set; }
        public ChallengeResult Result { get; set; }

        public override ChallengeAttemptDTO ToChallengeAttemptDTO() => new TriviaChallengeAnswerDTO
        {
            Id = Id,
            ChallengeId = ChallengeId,
            CreatedAt = CreatedAt,
            Result = Result,
            Username = Owner.UserName ?? OwnerId
        };
}