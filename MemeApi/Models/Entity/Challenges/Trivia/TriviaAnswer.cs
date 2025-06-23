namespace MemeApi.Models.Entity.Challenges;

public record TriviaAnswer : ChallengeAttempt
{
        public int AnswerOptionIndex { get; set; }
        public ChallengeResult Result { get; set; }
}