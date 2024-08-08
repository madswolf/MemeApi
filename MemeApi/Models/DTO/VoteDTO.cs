using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO;




/// <param name="ElementIDs"></param>
/// <param name="UpVote"></param>///<Summary>
/// A DTO for Votes
///</Summary>
public record VoteDTO
{
    /// <summary>
    /// Elements(s) to vote on, multiple means it is a vote on a meme.
    /// </summary>
    [Required]
    public List<string> ElementIDs { get; init; }

    /// <summary>
    /// Represents the status of the vote.
    /// </summary>
    [Required]
    public Upvote UpVote { get; init; }
}
