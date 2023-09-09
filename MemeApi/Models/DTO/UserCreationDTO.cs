using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO;

/// <summary>
/// A DTO for creating a new user
/// </summary>
/// <param name="Username"> Username of the user </param>
/// <param name="Email"> Email of the user </param>
/// <param name="Password"> Password of the user </param>
public record UserCreationDTO([property: Required] string Username, [property: Required] string Email, [property: Required] string Password);
