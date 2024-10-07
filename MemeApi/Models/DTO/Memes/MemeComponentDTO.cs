namespace MemeApi.Models.DTO.Memes;

/// <summary>
/// A DTO for meme components
/// </summary>
/// <param name="data"> Data/Content of the component </param>
/// <param name="id"> ID of the component </param>
public record MemeComponentDTO(string data, string id);
