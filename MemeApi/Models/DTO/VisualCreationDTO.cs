using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO;

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
    /// The texts position
    /// </summary>

    /// <summary>
    /// The list of topics that the Visual belongs to.
    /// </summary>
    public IEnumerable<string>? Topics { get; set; }
}