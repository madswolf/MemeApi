using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using SkiaSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MemeApi.library.Extensions;

public static class MemePlaceExtensions
{
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
        PixelChangeCount = submission.SubmissionPixelChanges.Count(),
    };

    public static byte[] ToRenderedPlaceImage(this MemePlace place)
    {
        var pixels = new Dictionary<Coordinate, Color>();

        place.PlaceSubmissions.OrderBy(ps => ps.CreatedAt).ToList().ForEach(ps =>
        {
            ps.SubmissionPixelChanges.ForEach(pixelChange =>
            {
                pixels[pixelChange.Coordinate] = pixelChange.Color;
            });
        });


        using var bitmap = new SKBitmap(place.width, place.height);
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(SKColors.White);


        foreach (var entry in pixels)
        {
            Coordinate coord = entry.Key;
            Color color = entry.Value;

            var paint = new SKPaint
            {
                Color = new SKColor(color.Red, color.Green, color.Blue),
                IsAntialias = true
            };

            if (coord.X < place.width && coord.Y < place.height)
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
}
