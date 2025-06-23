using System.Collections.Generic;

namespace MemeApi.Models.Entity.Challenges;

public record TriviaQuestion : Challenge
{
        public string Question { get; set; }
        public TriviaOption CorrectOption { get; set; }
        public List<TriviaOption> Options { get; set; }
}