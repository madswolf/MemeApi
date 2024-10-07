using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace MemeApi.library.Extensions;

public static class ExifExtensions
{
    public static byte[] WriteExifComment(this byte[] imageData, string comment)
    {
        comment = comment.Replace(' ', '_');
        comment = comment.Replace(':', 'x');
        using var image = Image.Load(imageData);
        if (image.Metadata.ExifProfile == null)
        {
            image.Metadata.ExifProfile = new ExifProfile();
        }

        image.Metadata.ExifProfile.SetValue(ExifTag.UserComment, comment);

        using var memoryStream = new MemoryStream();
        image.SaveAsPng(memoryStream);
        return memoryStream.ToArray();


    }

    public static string? GetExifComment(this byte[] imageData)
    {
        using var image = Image.Load(imageData);
        var exifProfile = image.Metadata.ExifProfile;

        if (exifProfile != null && exifProfile.TryGetValue(ExifTag.UserComment, out var userCommentValue))
        {
            var comment = userCommentValue?.ToString();
            if (comment != null && comment.StartsWith("UNICODE", StringComparison.OrdinalIgnoreCase))
            {
                comment = comment.Replace("UNICODE", "").Trim();
            }
            comment = comment.Replace('_', ' ');
            comment = comment.Replace('x', ':');
            return comment;
        }

        return null;
    }
}
