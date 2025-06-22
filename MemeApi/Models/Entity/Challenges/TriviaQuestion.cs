using System.Collections.Generic;

namespace MemeApi.Models.Entity.Challenges;

public class TriviaQuestion : Challenge
{
        public string Question { get; set; }
        public string[] Options { get; set; }
}