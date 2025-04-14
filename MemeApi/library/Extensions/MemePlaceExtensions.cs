using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MemeApi.Models.DTO.Places;
using MemeApi.Models.Entity;
using MemeApi.Models.Entity.Places;
using Microsoft.AspNetCore.Http;
using SkiaSharp;

namespace MemeApi.library.Extensions;

public static class MemePlaceExtensions
{
    public static DateTime? ParseDateTimeFromPlaceFileName(this string filename)
    {
        filename = filename.Replace("_", " ");
        filename = filename.Replace("x", ":");
        filename = filename.Replace(".png", "");

        string format = "yyyy-MM-dd HH:mm:ss";
        DateTime.TryParseExact(filename, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var datetime);
        return datetime;
    }

    public static bool IsBumpingForUser(this MemePlace place, User user, DateTime timeStamp)
    {
        var currentWeek = ISOWeek.GetWeekOfYear(timeStamp);
        
        return !place.PlaceSubmissions
            .Any(s =>
                s.OwnerId == user.Id &&
                ISOWeek.GetWeekOfYear(s.CreatedAt) == currentWeek
            );
    }
    
    public static double? SubmissionPriceForUser(this MemePlace place, int changedPixelsCount, User user, bool isBumpingSubmission)
    {
        var currentPixelPrice = place.CurrentPixelPrice();
        if (currentPixelPrice == null)
        {
            Console.WriteLine("Failed to get the current pixel price");
            return null;
        }
        
        var maxDubloonGain = 100;
        var dubloonGainPerDay = 100 / 7.0;

        var latestUserSubmission = place.LatestSubmissionByUser(user);
        
        var latestSubmissionDate = latestUserSubmission?.CreatedAt.Date ?? DateTime.UtcNow.AddDays(-8).Date;
        var currentDate = DateTime.UtcNow.Date;
        var daysSinceSubmission = Math.Max(0, (currentDate - latestSubmissionDate).Days);
        
        var dubloonGain = Math.Min(maxDubloonGain, daysSinceSubmission * dubloonGainPerDay);

        var bumpingPixelDiscount = isBumpingSubmission ? 200 : 0;
        var pixelChangePrice = Math.Max(0, changedPixelsCount - bumpingPixelDiscount) * currentPixelPrice.PricePerPixel;
        var requiredFunds = Math.Ceiling(pixelChangePrice - dubloonGain);
        
        return requiredFunds;
    }

    public static PlaceSubmission? LatestSubmissionByUser(this MemePlace place, User user)
    {
        return place.PlaceSubmissions
            .Where(p => p.OwnerId == user.Id)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefault();
    }

    public static PlacePixelPrice? CurrentPixelPrice(this MemePlace place)
    {
        return place.PriceHistory
            .OrderByDescending(s => s.PriceChangeTime)
            .FirstOrDefault();
    }

    public static PlaceSubmission? LatestSubmission(this MemePlace place)
    {
        return place.PlaceSubmissions.OrderByDescending(s => s.CreatedAt).FirstOrDefault(p => p.IsDeleted == false);
    }

    public static MemePlaceDTO ToMemePlaceDTO(this MemePlace place) => new MemePlaceDTO
    {
        Id = place.Id,
        Name = place.Name,
        PlaceSubmissions = place.PlaceSubmissions.Where(p => p.IsDeleted == false).Select(ps => ps.ToPlaceSubmissionDTO()).ToList(),
    };

    public static PlacePriceDTO ToPriceDTO(this PlacePixelPrice price) 
        => new()
    {
        Id = price.Id,
        PricePerPixel = price.PricePerPixel,
        PriceChangeTime = price.PriceChangeTime,
    };

    public static PlaceSubmissionDTO ToPlaceSubmissionDTO(this PlaceSubmission submission) => new()
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
                return currentPixelColor != pixelChange.Value;
            })
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public static bool IsBasedOnLatestRender(this IFormFile file, MemePlace? place)
    {
        var parsedDateTimeFromSubmissionImage = file.FileName.ParseDateTimeFromPlaceFileName();
        if (parsedDateTimeFromSubmissionImage == null) return false;
        
        var latestSubmission = place?.LatestSubmission();
       
        if (
            latestSubmission != null &&
            parsedDateTimeFromSubmissionImage?.TruncateToSeconds() < latestSubmission.CreatedAt.TruncateToSeconds())
            return false;

        return true;
    }

    public static byte[] ToRenderedSubmission( this Dictionary<Coordinate, Color> pixels, MemePlace place)
    {

        var baseImage = new SKBitmap(place.Width, place.Height);
        var canvas = new SKCanvas(baseImage);
        canvas.Clear(SKColors.Transparent);

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

        using var paint = new SKPaint
        {
            BlendMode = SKBlendMode.SrcOver // Explicitly setting the blend mode
        };

        canvas.DrawBitmap(baseBitmap, 0, 0, paint);

        canvas.DrawBitmap(overlayBitmap, 0, 0, paint);

        canvas.Flush();

        return baseBitmap;
    }

    public static byte[] ToByteArray(this SKBitmap bitMap)
    {
        using var stream = new MemoryStream();
        using var imageStream = new SKManagedWStream(stream);
        bitMap.Encode(imageStream, SKEncodedImageFormat.Png, quality: 100);
        return stream.ToArray().WriteExifComment(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
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
