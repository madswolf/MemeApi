#nullable disable warnings
using MemeApi;
using MemeApi.Models.Entity;
using MemeApi.Models.Entity.Memes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO.Memes;

/// <summary>
/// A DTO for creating text
/// </summary>
public record TextCreationDTO
{
    /// <summary>
    /// The textual content
    /// </summary>
    [Required]
    public string Text { get; init; }

    /// <summary>
    /// The texts position
    /// </summary>
    [Required]
    public MemeTextPosition Position { get; init; }

    /// <summary>
    /// The list of topics that the Text belongs to.
    /// </summary>
    public IEnumerable<string>? Topics { get; set; }
    /// <summary>
    /// An optional datetime for migrations.
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}