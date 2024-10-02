using MemeApi.Models.Entity;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CompactPixelListConverter : ValueConverter<List<Pixel>, string>
{
    public CompactPixelListConverter()
        : base(
            // Serialize: Convert List<Pixel> to compact string
            pixels => SerializePixels(pixels),

            // Deserialize: Convert compact string back to List<Pixel>
            str => DeserializePixels(str))
    {
    }

    // Method to serialize the pixels into a compact string
    private static string SerializePixels(List<Pixel> pixels)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var pixel in pixels)
        {
            sb.Append(pixel.Coordinate.X.ToString("D4")); // 4 digits for X
            sb.Append(pixel.Coordinate.Y.ToString("D4")); // 4 digits for Y
            sb.Append(pixel.Color.Red.ToString("D3"));    // 3 digits for Red
            sb.Append(pixel.Color.Green.ToString("D3"));  // 3 digits for Green
            sb.Append(pixel.Color.Blue.ToString("D3"));   // 3 digits for Blue
            sb.Append(pixel.Color.Alpha.ToString("D3"));  // 3 digits for Alpha
        }
        return sb.ToString();
    }

    // Method to deserialize the compact string back into a List<Pixel>
    private static List<Pixel> DeserializePixels(string str)
    {
        var pixels = new List<Pixel>();
        for (int i = 0; i < str.Length; i += 20)  // Each pixel is 20 characters
        {
            var pixel = new Pixel
            {
                Coordinate = new Coordinate
                {
                    X = int.Parse(str.Substring(i, 4)),     // First 4 characters: X
                    Y = int.Parse(str.Substring(i + 4, 4)) // Next 4 characters: Y
                },
                Color = new Color
                {
                    Red = byte.Parse(str.Substring(i + 8, 3)),   // Next 3 characters: Red
                    Green = byte.Parse(str.Substring(i + 11, 3)), // Next 3 characters: Green
                    Blue = byte.Parse(str.Substring(i + 14, 3)),  // Next 3 characters: Blue
                    Alpha = byte.Parse(str.Substring(i + 17, 3))  // Last 3 characters: Alpha
                }
            };
            pixels.Add(pixel);
        }
        return pixels;
    }
}
