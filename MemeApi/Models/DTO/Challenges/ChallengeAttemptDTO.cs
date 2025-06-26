using System;
using System.ComponentModel.DataAnnotations;
using MemeApi.Models.Entity.Challenges;

namespace MemeApi.Models.DTO.Challenges;

/// <summary>
/// A DTO for seeing ChallengeAttempts
/// </summary>
public abstract record ChallengeAttemptDTO
{
    /// <summary>
    /// Id of the attempt
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Id of the attempted challenge
    /// </summary>
    public string ChallengeId { get; set; }
    
    /// <summary>
    /// Attempt result
    /// </summary>
    public ChallengeResult Result { get; init; }
    
    /// <summary>
    /// The name of the user that attempted the challenge
    /// </summary>
    public string Username { get; set; }
    
    /// <summary>
    /// The time that the challenge was attempted
    /// </summary>
    public DateTime CreatedAt { get; set; }

}