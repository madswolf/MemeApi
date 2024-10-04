using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Http;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace MemeApi.library.Extensions;

public static class MemePlaceExtensions
{
    
    public static PlaceSubmission? LatestSubmission(this MemePlace place)
    {
        return place.PlaceSubmissions.OrderByDescending(s => s.CreatedAt).FirstOrDefault();
    }

    public static MemePlaceDTO ToMemePlaceDTO(this MemePlace place) => new MemePlaceDTO()
    {
        Id = place.Id,
        Name = place.Name,
        PlaceSubmissions = place.PlaceSubmissions.Select(ps => ps.ToPlaceSubmissionDTO()).ToList(),
    };

    public static PlaceSubmissionDTO ToPlaceSubmissionDTO(this PlaceSubmission submission) => new PlaceSubmissionDTO()
    {
        Id = submission.Id,
        CreatedAt = submission.CreatedAt,
        OwnerUserId = submission.Owner.Id,
        OwnerUserName = submission.Owner.UserName,
        PlaceId = submission.PlaceId,
        PixelChangeCount = submission.PixelChangeCount,
    };

    public static Dictionary<Coordinate, Color> ToSubmissionPixelChanges(this IFormFile file, byte[] placeRender)
    {
        using var unrederedPlaceBitmap = SKBitmap.Decode(placeRender);
        var unrederedPlace = unrederedPlaceBitmap.ToPixels();
        var unrenderedPlaceWithChanges = file.ToPixels();
        var defaultColor = new Color
        {
            Red = 255,
            Blue = 255,
            Green = 255,
            Alpha = 255,
        };

        return unrenderedPlaceWithChanges
            .Where(pixelChange => {
                unrederedPlace.TryGetValue(pixelChange.Key, out var currentPixelColor);
                var isDefault = pixelChange.Value == defaultColor && currentPixelColor == null;
                return currentPixelColor != pixelChange.Value && !isDefault;
            })
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public static bool IsBasedOnLatestRender(this IFormFile file, MemePlace? place)
    {
        var filename = file.FileName;
        if (filename == null) return false;

        var latestSubmission = place.LatestSubmission();

        filename = filename.Replace("_", " ");
        filename = filename.Replace("x", ":");
        filename = filename.Replace(".png", "");

        string format = "yyyy-MM-dd HH:mm:ss";
        var sucess = DateTime.TryParseExact(filename, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var renderedTimeOfSubmissionImage);

        if (!sucess ||
            latestSubmission != null &&
            (renderedTimeOfSubmissionImage.TruncateToSeconds() < latestSubmission.CreatedAt.TruncateToSeconds()))
            return false;

        return true;
    }

    public static byte[] ToRenderedSubmission( this Dictionary<Coordinate, Color> pixels, MemePlace place)
    {

        var baseImage = new SKBitmap(place.Width, place.Height);
        var canvas = new SKCanvas(baseImage);
        canvas.Clear(SKColors.White);

        foreach (var entry in pixels)
        {
            Coordinate coord = entry.Key;
            Color color = entry.Value;

            var paint = new SKPaint
            {
                Color = new SKColor(color.Red, color.Green, color.Blue, color.Alpha),
                BlendMode = SKBlendMode.SrcOver
            };

            canvas.DrawPoint(coord.X, coord.Y, paint);
        }

        using var stream = new MemoryStream();
        using var imageStream = new SKManagedWStream(stream);
        baseImage.Encode(imageStream, SKEncodedImageFormat.Png, quality: 100);
      
        return stream.ToArray();
    }

    public static SKBitmap OverlayImage(this SKBitmap baseBitmap, byte[] overlayImageBytes)
    {
        using var overlayBitmap = SKBitmap.Decode(overlayImageBytes);
        var imageInfo = new SKImageInfo(baseBitmap.Width, baseBitmap.Height);

        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;

        canvas.Clear(SKColors.White);

        canvas.DrawBitmap(baseBitmap, 0, 0);

        canvas.DrawBitmap(overlayBitmap, 0, 0);

        canvas.Flush();

        return baseBitmap;
    }

    public static byte[] ToByteArray(this SKBitmap bitMap)
    {
        using var stream = new MemoryStream();
        using var imageStream = new SKManagedWStream(stream);
        bitMap.Encode(imageStream, SKEncodedImageFormat.Png, quality: 100);
        return stream.ToArray();
    }

    public static byte[] ToByteArray(this IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        file.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    public static Dictionary<Coordinate, Color> ToPixels(this IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var skStream = new SKManagedStream(stream);
        using var bitmap = SKBitmap.Decode(skStream);
        return bitmap.ToPixels();
    }

    private static Dictionary<Coordinate, Color> ToPixels(this SKBitmap bitmap)
    {
        Dictionary<Coordinate, Color> coordinateColorMap = [];

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
