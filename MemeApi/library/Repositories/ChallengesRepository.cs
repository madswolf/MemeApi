using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MemeApi.library.Extensions;
using MemeApi.library.Services.Files;
using MemeApi.Models.Context;
using MemeApi.Models.DTO.Lotteries;
using MemeApi.Models.Entity;
using MemeApi.Models.Entity.Challenges;
using MemeApi.Models.Entity.Dubloons;
using MemeApi.Models.Entity.Lottery;
using Microsoft.EntityFrameworkCore;

namespace MemeApi.library.Repositories;

public class ChallengesRepository(MemeContext context, MemeApiSettings settings)
{
    public async Task<TriviaChallenge> CreateTriviaChallenge(string question, TriviaOption answer, List<TriviaOption> options)
    {
        var triviaChallenge = new TriviaChallenge
        {
            Id = Guid.NewGuid().ToString(),
            Category = ChallengeCategory.Trivia,
            Question = question,
            CorrectOption = answer,
            Options = options
        };
        
        context.TriviaChallenges.Add(triviaChallenge);
        await context.SaveChangesAsync();
        
        return triviaChallenge;
    }

    public async Task<Challenge?> GetChallenge(string challengeId)
    {
        return await context.Challenges
            .Include(challenge => challenge.Attempts)
            .ThenInclude(attempt => attempt.Owner)
            .Include(challenge => challenge.Attempts)
            .ThenInclude(attempt => attempt.DubloonEvent)
            .FirstOrDefaultAsync(challenge => challenge.Id == challengeId);
    }
    
    public async Task<TriviaChallenge?> GetTriviaChallenge(string triviaChallengeID)
    {
        return await context.TriviaChallenges
            .Include(triviaChallenge => triviaChallenge.Options)
            .FirstOrDefaultAsync(triviaChallenge => triviaChallenge.Id == triviaChallengeID);
    }

    public async Task<TriviaAnswer> AnswerTrivia(TriviaChallenge challenge, int answerIndex, User user)
    {
        var correctAnswer = challenge.CorrectOption.Index == answerIndex;
        
        var attempt = new TriviaAnswer
        {
            AttemptedChallenge = challenge,
            AnswerOptionIndex = answerIndex,
            Owner = user,
            Result = correctAnswer ? ChallengeResult.Succeeded : ChallengeResult.Failed
        };
    }
}