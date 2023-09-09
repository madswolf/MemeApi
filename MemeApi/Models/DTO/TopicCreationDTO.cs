using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO;

/// <summary>
/// A DTO for creating Topics
/// </summary>
/// <param name="TopicName"> Name of the topic </param>
/// <param name="Description"> Description of the topic </param>
public record TopicCreationDTO([property: Required] string TopicName, [property: Required] string Description);
