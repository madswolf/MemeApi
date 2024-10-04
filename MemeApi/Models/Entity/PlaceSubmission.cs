using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemeApi.Models.Entity;

public class PlaceSubmission
{
    public string Id { get; set; }

    public string OwnerId { get; set; }
    public User Owner { get; set; }

    public string PlaceId { get; set; }
    public MemePlace Place { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; }

    public int PixelChangeCount { get; set; }

}

public record Coordinate
{
    public int X { get; set; }
    public int Y { get; set; }
}

public record Color
{
    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }
    public byte Alpha { get; set; }
}