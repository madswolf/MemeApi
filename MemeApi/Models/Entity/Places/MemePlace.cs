using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemeApi.Models.Entity.Places;

public class MemePlace
{
    public string Id { get; set; }
    public string Name { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public List<PlaceSubmission> PlaceSubmissions { get; set; }
    public List<PlacePixelPrice> PriceHistory { get; set; }
}
