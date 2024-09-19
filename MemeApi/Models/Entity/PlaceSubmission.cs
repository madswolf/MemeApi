using System.Collections.Generic;

namespace MemeApi.Models.Entity;

public class PlaceSubmission
{
    public string id { get; set; }

    public string OwnerId { get; set; }
    public User Owner { get; set; }

    public string PlaceId { get; set; }
    public MemePlace Place { get; set; }

    public List<Pixel> SubmissionPixelChanges { get; set; }
}

public class Pixel
{
    public Coordinate Coordinate { get; set; }
    public Color Color { get; set; }
}

public class Coordinate
{
    public int X { get; set; }
    public int Y { get; set; }
}

public class Color
{
    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }
}