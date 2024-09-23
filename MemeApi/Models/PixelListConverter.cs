using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.Linq;
using MemeApi.Models.Entity;

public class CompactPixelListConverter : ValueConverter<List<Pixel>, string>
{
    public CompactPixelListConverter()
        : base(
            // Serialize: Convert List<Pixel> to array of arrays
            pixels => JsonSerializer.Serialize(
                pixels.Select(p => new object[]
                {
                    p.Coordinate.X, p.Coordinate.Y,
                    p.Color.Red, p.Color.Green, p.Color.Blue, p.Color.Alpha
                }),
                (JsonSerializerOptions)null),

            // Deserialize: Convert array of arrays back to List<Pixel>
            json => JsonSerializer.Deserialize<List<JsonElement[]>>(json, (JsonSerializerOptions)null)
                .Select(arr => new Pixel
                {
                    Coordinate = new Coordinate
                    {
                        X = arr[0].GetInt32(),
                        Y = arr[1].GetInt32()
                    },
                    Color = new Color
                    {
                        Red = arr[2].GetByte(),
                        Green = arr[3].GetByte(),
                        Blue = arr[4].GetByte(),
                        Alpha = arr[5].GetByte()
                    }
                }).ToList())
    {
    }
}
