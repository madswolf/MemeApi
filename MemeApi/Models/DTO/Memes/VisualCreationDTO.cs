using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO.Memes;

/// <summary>
/// A DTO for creating text
/// </summary>
public record VisualCreationDTO
{
    /// <summary>
    /// The textual content
    /// </summary>
    [Required]
    public IFormFile File { get; init; }

    /// <summary>
    /// Optional name for the visual component
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// The list of topics that the Visual belongs to.
    /// </summary>
    public List<string>? Topics { get; set; }

    /// <summary>
    /// An optional datetime for migrations.
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}