#nullable disable warnings
using System;
using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO;

/// <summary>
/// A DTO for creating a new user.
/// </summary>
public record UserCreationDTO
{
    /// <summary>
    /// Username of the user.
    /// </summary>
    [Required]
    public string Username { get; init; }

    /// <summary>
    /// Email of the user.
    /// </summary>
    [Required]
    public string Email { get; init; }

    /// <summary>
    /// Password of the user.
    /// </summary>
    [Required]
    public string Password { get; init; }

    /// <summary>
    /// An optional datetime for migrations.
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}