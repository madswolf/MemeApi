using Microsoft.AspNetCore.Http;

namespace MemeApi.Models.DTO.Places;

public class PlaceSubmissionCreationDTO
{
    public IFormFile ImageWithChanges { get; set; }
    public string PlaceId { get; set; }
}