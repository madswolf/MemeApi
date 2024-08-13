using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO;




/// <param name="ElementIDs"></param>
/// <param name="UpVote"></param>///<Summary>
/// <param name="VoteNumber"></param>///<Summary>
/// A DTO for Votes
///</Summary>
public record PostVoteDTO
{
    /// <summary>
    /// Elements(s) to vote on, multiple means it is a vote on a meme.
    /// </summary>
    [Required]
    public List<string> ElementIDs { get; init; }

    /// <summary>
    /// Represents the status of the vote.
    /// </summary>
    public Upvote? UpVote { get; set; }

    /// <summary>
    /// Represents the integer value of the vote. Either This or Upvote needs to be set.
    /// </summary>
    [Range(0, 9, ErrorMessage = "VoteNumber must be between 0 and 9.")]
    public int? VoteNumber { get; set; }

}
