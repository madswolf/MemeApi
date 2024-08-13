﻿using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Http;
using System;
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
    /// The list of topics that the Visual belongs to.
    /// </summary>
    public IEnumerable<string>? Topics { get; set; }

    /// <summary>
    /// An optional datetime for migrations.
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}