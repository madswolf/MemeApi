using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MemeApi.Models.Entity.Challenges;

namespace MemeApi.Models.DTO.Challenges;

/// <summary>
/// A DTO for Challenges
/// </summary>
public abstract record ChallengeDTO
{
    /// <summary>
    /// Category of the Challenge
    /// </summary>
    [Required]
    public ChallengeCategory Category {get; init; }

    /// <summary>
    /// Challenge Attempts
    /// </summary>
    [Required]
    public List<ChallengeAttemptDTO> Attempts { get; init; }
}