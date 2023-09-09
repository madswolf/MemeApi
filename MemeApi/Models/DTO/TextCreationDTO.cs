using MemeApi.Models.Entity;
using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO;


/// <summary>
/// A DTO for creating text
/// </summary>
/// <param name="Text"> The textual content </param>
/// <param name="position"> The texts position </param>
public record TextCreationDTO([property: Required] string Text, [property: Required] MemeTextPosition position);
