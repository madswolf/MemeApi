using System;

namespace MemeApi.Models.DTO.Memes;

/// <summary>
/// A DTO for a random meme or component.
/// </summary>
/// <param name="data"> The data or content of the component </param>
/// <param name="id"> The components ID </param>
/// <param name="voteAverage"> The vote score of the component </param>
/// <param name="createdAt"> The creation time of the component </param>
/// <param name="owner"> The owner of the component </param>
public record VotableComponentDTO(string data, string id, double voteAverage, DateTime createdAt, string owner);
