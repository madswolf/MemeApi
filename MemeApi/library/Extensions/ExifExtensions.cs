using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace MemeApi.library.Extensions;

public static class ExifExtensions
{
    public static string SanitizeString(this string inputString)
    {
        var output = inputString.Replace(' ', '_');
        output = output.Replace(':', 'x');
        return output;
    }

    public static string DeSanitizeString(this string inputString)
    {
        var output = inputString.Replace('_', ' ');
        output = output.Replace('x', ':');
        return output;
    }
    public static byte[] WriteExifComment(this byte[] imageData, string comment)
    {
        using var image = Image.Load(imageData);
        if (image.Metadata.ExifProfile == null)
        {
            image.Metadata.ExifProfile = new ExifProfile();
        }

        image.Metadata.ExifProfile.SetValue(ExifTag.UserComment, comment.SanitizeString());

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

            if (comment != null)
            {
                if(comment.StartsWith("UNICODE", StringComparison.OrdinalIgnoreCase))
                    comment = comment.Replace("UNICODE", "").Trim();
                comment = comment.DeSanitizeString();
            }
            
            return comment;
        }

        return null;
    }
}
