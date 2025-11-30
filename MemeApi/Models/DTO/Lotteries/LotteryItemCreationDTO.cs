using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MemeApi.Models.DTO.Lotteries;

/// <summary>
/// A DTO for Lottery Items
/// </summary>
public record LotteryItemCreationDTO
{
    /// <summary>
    /// Name of the Lottery Item
    /// </summary>
    [Required]
    public string ItemName { get; init; } 
    
    /// <summary>
    /// Count of Lottery items
    /// </summary>
    [Required]
    public int ItemCount { get; init; }
    
    /// <summary>
    /// The thumbnail of the Item
    /// </summary>
    [Required]
    public IFormFile ItemThumbnail { get; init; }
    
    /// <summary>
    /// The image of the Item
    /// </summary>
    public IFormFile? ItemImage { get; init; }
}