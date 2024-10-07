using System;

namespace MemeApi.Models.DTO.Places;

public class PlaceSubmissionDTO
{
    public string Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string OwnerUserName { get; set; }
    public string OwnerUserId { get; set; }
    public string PlaceId { get; set; }
    public int PixelChangeCount { get; set; }
}