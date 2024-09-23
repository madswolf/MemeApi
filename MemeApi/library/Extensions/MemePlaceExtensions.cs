using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Http;
using SkiaSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MemeApi.library.Extensions;

public static class MemePlaceExtensions
{
    public static PlaceSubmission LatestSubmission(this MemePlace place)
    {
        return place.PlaceSubmissions.OrderByDescending(s => s.CreatedAt).First();
    }
    public static MemePlaceDTO ToMemePlaceDTO(this MemePlace place) => new MemePlaceDTO()
    {
        Id = place.Id,
        PlaceSubmissions = place.PlaceSubmissions.Select(ps => ps.ToPlaceSubmissionDTO()).ToList(),
    };

    public static PlaceSubmissionDTO ToPlaceSubmissionDTO(this PlaceSubmission submission) => new PlaceSubmissionDTO()
    {
        Id = submission.Id,
        CreatedAt = submission.CreatedAt,
        OwnerUserId = submission.Owner.Id,
        OwnerUserName = submission.Owner.UserName,
        PlaceId = submission.PlaceId,
        PixelChangeCount = submission.PixelSubmissions.Count,
    };

    public static Dictionary<Coordinate, Color> ToSubmissionPixelChanges(this IFormFile file, MemePlace place)
    {
        var unrederedPlace = place.ToUnrenderedPlacePixels();
        var unrenderedPlaceWithChanges = file.ToUnrenderedPlacePixels();
        var defaultColor = new Color
        {
            Red = 255,
            Blue = 255,
            Green = 255,
            Alpha = 255,
        };

        return unrenderedPlaceWithChanges
            .Where(pair => {
                unrederedPlace.TryGetValue(pair.Key, out var value);
                return pair.Value != defaultColor && value != pair.Value;
                })
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public static byte[] ToRenderedSubmission(this PlaceSubmission submission)
    {
        var minX = submission.PixelSubmissions.Min(pixel => pixel.Coordinate.X);
        var minY = submission.PixelSubmissions.Min(pixel => pixel.Coordinate.Y);
        var maxX = submission.PixelSubmissions.Max(pixel => pixel.Coordinate.X);
        var maxY = submission.PixelSubmissions.Max(pixel => pixel.Coordinate.Y);

        var width = maxX - minX;
        var height = maxY - minY;

        Dictionary<Coordinate, Color> pixels =
            submission.PixelSubmissions.ToDictionary(ps => new Coordinate
            {
                X = ps.Coordinate.X - minX,
                Y = ps.Coordinate.Y - minY,
            },
            ps => ps.Color);
        return RenderBitsmap(width, height, pixels);
    }

    public static byte[] ToRenderedPlace(this MemePlace place)
    {
        Dictionary<Coordinate, Color> pixels = place.ToUnrenderedPlacePixels();

        return RenderBitsmap(place.Width, place.Height, pixels);
    }

    private static byte[] RenderBitsmap(int width, int height, Dictionary<Coordinate, Color> pixels)
    {
        var bitmap = new SKBitmap(width, height);
        var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);


        foreach (var entry in pixels)
        {
            Coordinate coord = entry.Key;
            Color color = entry.Value;

            var paint = new SKPaint
            {
                Color = new SKColor(color.Red, color.Green, color.Blue, color.Alpha),
                IsAntialias = true
            };

            if (coord.X < width && coord.Y < height)
            {
                canvas.DrawPoint(coord.X, coord.Y, paint);
            }
        }

        using (var stream = new MemoryStream())
        {
            using (var imageStream = new SKManagedWStream(stream))
            {
                bitmap.Encode(imageStream, SKEncodedImageFormat.Png, quality: 100);
            }

            return stream.ToArray();
        }
    }

    public static Dictionary<Coordinate, Color> ToUnrenderedPlacePixels(this MemePlace place)
    {
        var pixels = new Dictionary<Coordinate, Color>();

        place.PlaceSubmissions.OrderBy(ps => ps.CreatedAt).ToList().ForEach(ps =>
        {
            foreach (var pair in ps.PixelSubmissions)
            {
                pixels[pair.Coordinate] = pair.Color;
            }
        });
        return pixels;
    }

    public static Dictionary<Coordinate, Color> ToUnrenderedPlacePixels(this IFormFile file)
    {
        Dictionary<Coordinate, Color> coordinateColorMap = [];

        using var stream = file.OpenReadStream();
        using var skStream = new SKManagedStream(stream);
        using var bitmap = SKBitmap.Decode(skStream);

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                SKColor skColor = bitmap.GetPixel(x, y);

                var color = new Color
                {
                    Red = skColor.Red,
                    Green = skColor.Green,
                    Blue = skColor.Blue,
                    Alpha = skColor.Alpha
                };

                var coord = new Coordinate
                {
                    X = x,
                    Y = y
                };

                coordinateColorMap[coord] = color;
            }
        }

        return coordinateColorMap;
    }
}
