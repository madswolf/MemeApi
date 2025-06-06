﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MemeApi.library;
using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.library.Services.Files;
using MemeApi.Models.DTO.Memes;
using MemeApi.Models.Entity.Memes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace MemeApi.Controllers;


/// <summary>
/// A controller for creating memes made of visuals and textual components.
/// </summary>
[Route("[controller]")]
[ApiController]
public class MemesController : ControllerBase
{
    private readonly IMemeRenderingService _memeRendererService;
    private readonly MemeRepository _memeRepository;
    private readonly MemeApiSettings _settings;

    /// <summary>
    /// A controller for creating memes made of visuals and textual components.
    /// </summary>
    public MemesController(MemeRepository memeRepository, IMemeRenderingService memeRendererService, MemeApiSettings settings)
    {
        _memeRepository = memeRepository;
        _memeRendererService = memeRendererService;
        _settings = settings;
    }
    /// <summary>
    /// Get all memes
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemeDTO>>> GetMemes()
    {
        var memes = await _memeRepository.GetMemes();
        return Ok(memes.Select(m => m.ToMemeDTO(_settings.GetMediaHost())));
    }

    /// <summary>
    /// Get a specific meme by ID
    /// </summary> 
    [HttpGet("{id}")]
    public async Task<ActionResult<MemeDTO>> GetMeme(string id)
    {
        var meme = await _memeRepository.GetMeme(id);
        if (meme == null) return NotFound();

        return Ok(meme.ToMemeDTO(_settings.GetMediaHost()));
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
    public async Task<ActionResult<MemeDTO>> PostMeme([FromForm]MemeCreationDTO memeCreationDto, [FromQuery] bool? renderMeme = false)
    {
        if (!memeCreationDto.VisualFile.FileName.Equals("VisualFile")) memeCreationDto.FileName = memeCreationDto.VisualFile.FileName;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var meme = await _memeRepository.CreateMeme(memeCreationDto, userId);
        if (meme == null) return NotFound("One of the topics was not found");

        var renderedMeme = renderMeme == true ? await _memeRendererService.RenderMeme(meme) : null;  
        return CreatedAtAction(nameof(GetMeme), new { id = meme.Id }, meme.ToMemeDTO(_settings.GetMediaHost(), renderedMeme));
    }

    [HttpPost("ById")]
    [AllowAnonymous]
    public async Task<ActionResult<MemeDTO>> PostMemeById([FromBody] MemeCreationByIdDTO memeCreationDto)
    {     
        var meme = await _memeRepository.CreateMemeById(memeCreationDto);
        if (meme == null) return NotFound("One of the topics was not found");

        return CreatedAtAction(nameof(GetMeme), new { id = meme.Id }, meme.ToMemeDTO(_settings.GetMediaHost()));
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
            .Where(x => !regex.IsMatch(x.Visual.Filename))
            .Where(x => x.TopText == null || x.TopText.Text.Length < 150)
            .Where(x => x.BottomText == null || x.BottomText.Text.Length < 150)
            .ToList();
        return Ok(list.RandomItem(seed));
    }

    /// <summary>
    /// Get a random meme rendered to a png in the response
    /// Use the optional Query parameters TopText and BottomText to define what the top and bottom text should be
    /// </summary>
    [HttpGet("random/Rendered")]
    public async Task<ActionResult> RenderImage([FromQuery] string? VisualId = null, [FromQuery] string? TopText = null, [FromQuery] string? BottomText = null)
    {
        var meme = await _memeRepository.RandomMemeByComponents(VisualId, TopText, BottomText);
        var jsonResponse = JsonConvert.SerializeObject(meme.ToMemeDTO(_settings.GetMediaHost()));
        var cleanedHeaderValue = Regex.Replace(jsonResponse, @"[^\x20-\x7E]", "X");

        var watch = Stopwatch.StartNew();

        var file = File(await _memeRendererService.RenderMeme(meme), "image/png");

        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Console.WriteLine(elapsedMs);


        Response.Headers.Append(new("X-File-Info", cleanedHeaderValue));
        Response.Headers[HeaderNames.ContentDisposition] = new ContentDispositionHeaderValue("inline")
        {
            FileNameStar = meme.ToFilenameString()
        }.ToString();
        Response.Headers.Append(new ("X-File-Render-Time", elapsedMs + "ms"));

        return file;
    }

    /// <summary>
    /// Get a random meme rendered to a png in the response
    /// Use the optional Query parameters TopText and BottomText to define what the top and bottom text should be
    /// </summary>
    [HttpGet("Render")]
    public ActionResult RenderImage([FromForm] MemeCreationDTO memeDTO)
    {
        return File(_memeRendererService.RenderMemeFromData(memeDTO.VisualFile.ToByteArray(), memeDTO.TopText, memeDTO.BottomText), "image/png");
    }
}

