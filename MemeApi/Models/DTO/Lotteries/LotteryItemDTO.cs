using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO.Lotteries;

/// <summary>
/// A DTO for Lottery Items
/// </summary>
public record LotteryItemDTO
{
    /// <summary>
    /// Id of the Lottery Item
    /// </summary>
    public string ItemId { get; init; } 
    
    /// <summary>
    /// Name of the Lottery Item
    /// </summary>
    public string ItemName { get; init; } 
    
    /// <summary>
    /// Initial count of Lottery items
    /// </summary>
    public int InitialItemCount { get; init; }
    
    /// <summary>
    /// Current count of Lottery items
    /// </summary>
    public int CurrentItemCount { get; init; }

    
    /// <summary>
    /// The weight of the probability for the Item
    /// </summary>
    public int ItemProbabilityWeight { get; init; }
    
    /// <summary>
    /// The weight of the probability for the Item
    /// </summary>
    public string ItemThumbnail { get; init; }
}