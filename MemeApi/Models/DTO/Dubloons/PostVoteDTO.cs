using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO.Dubloons;




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

    /// <summary>
    /// An Identifier supplied from an external provider.
    /// Only applicable for ingegrations where an application is voting on behalf of a user.
    /// </summary>
    public string? ExternalUserID { get; set; }
    /// <summary>
    /// Username used on the external provider.
    /// Only applicable for ingegrations where an application is voting on behalf of a user.
    /// </summary>
    public string? ExternalUserName { get; set; }

}
