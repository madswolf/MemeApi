using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace MemeApi.Models.DTO;

public class MemeOfTheDayScheduleDTO : IValidatableObject
{
    [Required]
    public string Start { get; set; } = "";
    public string? End { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!TimeOnly.TryParseExact(Start, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start) ||
            !TimeOnly.TryParseExact(End ?? Start, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
        {
            yield return new ValidationResult("Use 24-hour HH:mm times.", [nameof(Start), nameof(End)]);
        }
        else if (end < start)
        {
            yield return new ValidationResult("End must be at or after start on the same day.", [nameof(End)]);
        }
    }
}
