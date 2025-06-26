using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MemeApi.library;
using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.library.Repositories;
using MemeApi.Models.DTO.Lotteries;
using MemeApi.Models.Entity.Lottery;
using Microsoft.AspNetCore.Mvc;

namespace MemeApi.Controllers;

/// <summary>
/// A controller for Lotteries.
/// </summary>
[Route("[controller]")]
[ApiController]
public class ChallengesController(
    ChallengesRepository challengesRepository,
    MemeApiSettings settings,
    UserRepository userRepository)
    : ControllerBase
{
    private readonly UserRepository _userRepository = userRepository;

    // create a lottery with a name and a list of items with probabilities, urls and such
    /// <summary>
    /// Create a Lottery
    /// </summary>
    [HttpPost("{challengeId}")]
    public async Task<ActionResult> GetChallenges(string challengeId)
    {
        var challenge = await challengesRepository.GetChallenge(challengeId);
        if (challenge == null) return NotFound(challengeId);
        
        return Ok(challenge);
    }
}