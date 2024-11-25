using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO.Lotteries;

/// <summary>
/// A DTO for creating Lotteries
/// </summary>
public record LotteryDTO
{
    /// <summary>
    /// Id of the Lottery
    /// </summary>
    public string Id { get; init; }
    
    /// <summary>
    /// Name of the Lottery
    /// </summary>
    public string Name { get; init; }
    
    /// <summary>
    /// Price of tickets for the lottery
    /// </summary>
    public int TicketCost { get; init; }
    
    public List<LotteryItemDTO> Items { get; init; }
}