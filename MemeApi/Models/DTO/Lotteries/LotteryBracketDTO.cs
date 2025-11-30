using System.Collections.Generic;

namespace MemeApi.Models.DTO.Lotteries;

/// <summary>
/// A DTO for Lottery Item Brackets
/// </summary>
public record LotteryBracketDTO
{
    /// <summary>
    /// Id of the Lottery Bracket
    /// </summary>
    public string BracketId { get; init; } 
    
    /// <summary>
    /// Name of the Lottery Bracket
    /// </summary>
    public string BracketName { get; init; }
    
    /// <summary>
    /// The weight of the probability for the Bracket
    /// </summary>
    public int BracketProbabilityWeight { get; init; }
    
    /// <summary>
    /// The Color of the Bracket in HEX 
    /// </summary>
    public string Color { get; init; }
    
    /// <summary>
    /// List of Lottery Items in the Bracket
    /// </summary>
    public List<LotteryItemDTO> Items { get; init; }
}