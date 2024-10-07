using System.Collections.Generic;

namespace MemeApi.Models.DTO.Places;

public class MemePlaceDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<PlaceSubmissionDTO> PlaceSubmissions { get; set; }
}
