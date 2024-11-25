using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO.Lotteries;

/// <summary>
/// A DTO for creating Lotteries
/// </summary>
public record LotteryCreationDTO
{
    /// <summary>
    /// Name of the Lottery
    /// </summary>
    [Required]
    public string LotteryName { get; init; }
    
    /// <summary>
    /// Price of tickets for the lottery
    /// </summary>
    [Required]
    public int TicketCost { get; init; }
    
    /// <summary>
    /// List of Item Brackets for the lottery
    /// </summary>
    [Required]
    public List<(string BracketName, int ProbabilityWeight)> Brackets { get; init; }
}