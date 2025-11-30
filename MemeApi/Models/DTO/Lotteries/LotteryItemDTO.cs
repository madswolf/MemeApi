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
    /// The thumbnail of the Item
    /// </summary>
    public string ItemThumbnail { get; init; }
    
    /// <summary>
    /// The color for of the rarity of the Item
    /// </summary>
    public string ItemRarityColor { get; init; }
    
    /// <summary>
    /// A boolean signifying if the given Item is out of stock
    /// </summary>
    public bool OutOfStock { get; init; }
}