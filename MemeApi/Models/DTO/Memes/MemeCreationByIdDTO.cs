#nullable disable warnings
using System;
using System.Collections.Generic;

namespace MemeApi.Models.DTO.Memes;

/// <summary>
/// A DTO with information to create a meme 
/// </summary>
public class MemeCreationByIdDTO
{
    /// <summary>
    /// The id of the visual component of the meme
    /// </summary>
    public string VisualId { get; set; }
    /// <summary>
    /// The id of textual top component of the meme
    /// </summary>
    public string? TopTextId { get; set; }
    /// <summary>
    /// The id of textual bottom component of the meme
    /// </summary>
    public string? BottomTextId { get; set; }
    /// <summary>
    /// The list of topics that the Meme belongs to.
    /// </summary>
    public IEnumerable<string>? Topics { get; set; }
    /// <summary>
    /// An optional datetime for migrations.
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}