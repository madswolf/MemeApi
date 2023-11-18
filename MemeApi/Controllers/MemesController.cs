using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using SkiaSharp;

namespace MemeApi.Controllers;


/// <summary>
/// A controller for creating memes made of visuals and textual components.
/// </summary>
[Route("[controller]")]
[ApiController]
public class MemesController : ControllerBase
{
    private readonly MemeRepository _memeRepository;
    /// <summary>
    /// A controller for creating memes made of visuals and textual components.
    /// </summary>
    public MemesController(MemeRepository memeRepository)
    {
        _memeRepository = memeRepository;
    }
    /// <summary>
    /// Get all memes
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemeDTO>>> GetMemes()
    {
        var memes = await _memeRepository.GetMemes();
        return Ok(memes.Select(m => m.ToMemeDTO()));
    }

    /// <summary>
    /// Get a specific meme by ID
    /// </summary> 
    [HttpGet("{id}")]
    public async Task<ActionResult<MemeDTO>> GetMeme(string id)
    {
        var meme = await _memeRepository.GetMeme(id);
        if (meme == null) return NotFound();

        return Ok(meme.ToMemeDTO());
    }

    private SKCanvas DrawText(SKCanvas canvas, string text, int canvasWidth, float centerY, SKTypeface font, int textSize)
    {
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

        float centerX = (canvasWidth - textPaint.MeasureText(text)) / 2; // Centered X-coordinate

        canvas.DrawText(text, centerX, centerY, outlinePaint);
        canvas.DrawText(text, centerX, centerY, textPaint);

        return canvas;
    }

    [HttpGet("test/{toptext}/{bottomtext}")]
    public ActionResult RenderImage(string topText, string bottomText)
    {
        // Load the existing image from disk
        // var imagePath = Path.Combine("wwwroot", "images", imageName); // Adjust the path as needed
        // if (!System.IO.File.Exists(imagePath))
        // {
        //    return NotFound();
        // }

        SKImageInfo info = new SKImageInfo(400, 400, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var inputImage = SKBitmap.Decode("C:\\code\\MemeApi\\MemeApi\\uploads\\image0.jpg");
        using var resized = inputImage.Resize(info, SKFilterQuality.High);

        var canvas = new SKCanvas(resized);
        var textSize = 40;

        canvas.DrawBitmap(resized, new SKPoint(0, 0));

        float topTextY = resized.Height/6   ;
        DrawText(canvas, topText, resized.Width, topTextY, SKTypeface.FromFamilyName("Impact"), textSize);

        float bottomTextY = resized.Height - resized.Height/8;
        DrawText(canvas, bottomText, resized.Width, bottomTextY, SKTypeface.FromFamilyName("Impact"), textSize);

        using (var stream = new MemoryStream())
        {
            using (var imageStream = new SKManagedWStream(stream))
            {
                resized.Encode(imageStream, SKEncodedImageFormat.Png, quality: 100);
            }

            return File(stream.ToArray(), "image/png");
        }
    }

    private SKBitmap testImage()
    {
        // Create an SKImageInfo specifying the dimensions and color type (e.g., RGBA8888)
        SKImageInfo info = new SKImageInfo(800, 600, SKColorType.Rgba8888, SKAlphaType.Premul);

        // Create an SKBitmap with the specified image info
        using SKBitmap bitmap = new SKBitmap(info);
        // Create an SKCanvas to draw on the SKBitmap
        using SKCanvas canvas = new SKCanvas(bitmap);
        // Clear the canvas with a red background
        canvas.Clear(SKColors.Red);

        // Now, you have an SKBitmap with a red background that you can use for testing.
        return bitmap;

        // At this point, 'bitmap' contains your SKBitmap with the red background.
    }

    //[HttpPut("{id}")]
    //public async Task<IActionResult> PutMeme(int id, Meme meme)
    //{
    //    if (id != meme.Id)
    //    {
    //        return BadRequest();
    //    }

    //    if (await _memeRepository.ModifyMeme(id, meme)) return NotFound();

    //    return NoContent();
    //}

    /// <summary>
    /// Create a meme
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<MemeDTO>> PostMeme([FromForm]MemeCreationDTO memeCreationDto)
    {
        if (!memeCreationDto.VisualFile.FileName.Equals("VisualFile")) memeCreationDto.FileName = memeCreationDto.VisualFile.FileName;
        var meme = await _memeRepository.CreateMeme(memeCreationDto);
        if (meme == null) return NotFound("One of the topics was not found");
        return CreatedAtAction(nameof(GetMeme), new { id = meme.Id }, meme.ToMemeDTO());
    }

    /// <summary>
    /// Delete a meme
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMeme(string id)
    {
        if (await _memeRepository.DeleteMeme(id)) return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Get a random meme based on an optional seed
    /// </summary>
    [HttpGet]
    [Route("random/{seed?}")]
    public async Task<ActionResult<Meme>> RandomMeme(string seed = "")
    {
        var list = await _memeRepository.GetMemes();
        var regex = new Regex("^.*\\.gif$");
        list = list
            .Where(x => !regex.IsMatch(x.MemeVisual.Filename))
            .Where(x => x.Toptext == null || x.Toptext.Text.Length < 150)
            .Where(x => x.BottomText == null || x.BottomText.Text.Length < 150)
            .ToList();
        return Ok(list.RandomItem(seed));
    }
}

