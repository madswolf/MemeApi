using System;

namespace MemeApi.Models.DTO.Places;

public class PlacePriceDTO
{
    public string Id { get; set; }
    public double PricePerPixel { get; set; }
    public DateTime PriceChangeTime { get; set; }
}
