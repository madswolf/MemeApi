using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MemeApi.library.Extensions;
using MemeApi.Models.Entity.Memes;
using SkiaSharp;

namespace MemeApi.library.Services.Files;

public class MemeRenderingService : IMemeRenderingService
{
    private readonly MemeApiSettings _settings;
    private readonly IFileLoader _loader;

    public MemeRenderingService(MemeApiSettings settings, IFileLoader loader)
    {
        _settings = settings;
        _loader = loader;
    }

    public async Task<byte[]> RenderMeme(Meme meme)
    {
        var path = Path.Combine("visual/", meme.Visual.Filename);
        var data = await _loader.LoadFile(path);

        return RenderMemeFromData(data, meme.ToTopText(), meme.ToBottomText());
    }

    public byte[] RenderMemeFromData(byte[] data, string? toptext = null, string? bottomtext = null)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"test.py",
            //RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        using var process = Process.Start(psi);
        try
        {
            process.Start();

            // Write input data to stdin
            //using (var stdin = process.StandardInput.BaseStream)
            //{
            //    stdin.Write(data, 0, data.Length);
            //}

            // Read output and error streams
            using var memoryStream = new MemoryStream();
            var stderrTask = process.StandardError.ReadToEndAsync();
            process.StandardOutput.BaseStream.CopyTo(memoryStream);

            process.WaitForExit();

            var stderr = stderrTask.Result;

            if (process.ExitCode != 0)
            {
                throw new Exception($"Python process exited with code {process.ExitCode}: {stderr}");
            }

            if (!string.IsNullOrWhiteSpace(stderr))
            {
                throw new Exception($"Python error output: {stderr}");
            }

            memoryStream.Position = 0;
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to render meme image using Python process", ex);
        }
    }
 

    private SKCanvas DrawText(SKCanvas canvas, string? text, int canvasWidth, float centerY, SKTypeface font, int textSize)
    {
        if (text == null) return canvas;
        using var textPaint = new SKPaint();
        textPaint.Color = SKColors.White;
        textPaint.TextSize = textSize;
        textPaint.Typeface = font;
        textPaint.IsAntialias = true;

        // Outline 
        using var outlinePaint = new SKPaint();
        outlinePaint.Color = SKColors.Black;
        outlinePaint.TextSize = textSize;
        outlinePaint.Typeface = font;
        outlinePaint.IsAntialias = true;
        outlinePaint.StrokeWidth = 4; // Width of the outline
        outlinePaint.Style = SKPaintStyle.Stroke;
        
        float centerX = (canvasWidth - textPaint.MeasureText(text)) / 2;

        canvas.DrawText(text, centerX, centerY, outlinePaint);
        canvas.DrawText(text, centerX, centerY, textPaint);

        return canvas;
    }
}
