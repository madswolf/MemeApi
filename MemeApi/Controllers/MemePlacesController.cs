using MemeApi.library;
using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.library.Repositories;
using MemeApi.library.Services.Files;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MemeApi.Controllers;

/// <summary>
/// A controller for creating and managing visual meme components.
/// </summary>
[Route("[controller]")]
[ApiController]
public class MemePlacesController : ControllerBase
{
    private readonly MemeApiSettings _settings;
    private readonly MemePlaceRepository _memePlaceRepository;
    private readonly UserRepository _userRepository;
    private readonly IFileSaver _fileSaver;

    /// <summary>
    /// A controller for creating and managing visual meme components.
    /// </summary>
    public MemePlacesController(MemePlaceRepository memePlaceRepository, MemeApiSettings settings, UserRepository userRepository, IFileSaver fileSaver)
    {
        _memePlaceRepository = memePlaceRepository;
        _settings = settings;
        _userRepository = userRepository;
        _fileSaver = fileSaver;
    }

    /// <summary>
    /// Create Place
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MemePlaceDTO>> CreateMemePlace([FromForm]PlaceCreationDTO placeCreationDTO)
    {
        var place = await _memePlaceRepository.CreateMemePlace(placeCreationDTO);
        return Ok(place.ToMemePlaceDTO());
    }

    /// <summary>
    /// Get all MemePlaces
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemePlaceDTO>>> GetMemePlaces()
    {
        var places = await _memePlaceRepository.GetMemePlaces();
        return Ok(places.Select(p => p.ToMemePlaceDTO()));
    }

    /// <summary>
    /// Get all PlaceSubmissions for given placeId
    /// </summary>
    [HttpGet("{placeId}/submissions/latest")]
    public async Task<ActionResult<PlaceSubmissionDTO>> GetLatestPlaceSubmission(string placeId)
    {
        var place = await _memePlaceRepository.GetMemePlace(placeId);
        if (place == null) return NotFound(placeId);

        var submission = place.LatestSubmission();
        if (submission == null) return NoContent();

        return Ok(submission.ToPlaceSubmissionDTO());
    }

    /// <summary>
    /// Get all PlaceSubmissions for given placeId
    /// </summary>
    [HttpGet("{placeId}/submissions")]
    public async Task<ActionResult<IEnumerable<PlaceSubmissionDTO>>> GetPlaceSubmissions(string placeId, [FromQuery] bool includeDeleted = false)
    {
        var places = await _memePlaceRepository.GetMemePlaceSubmissions(placeId, includeDeleted);
        return Ok(places.Select(p => p.ToPlaceSubmissionDTO()));
    }

    /// <summary>
    /// Get a rendered place submission
    /// </summary>
    [HttpGet("submissions/{submissionId}")]
    public async Task<ActionResult<IEnumerable<PlaceSubmissionDTO>>> GetRenderedPlaceSubmission(string submissionId)
    {
        var render = await _memePlaceRepository.GetPlaceSubmissionRender(submissionId);
        if(render == null) return NotFound(submissionId);

        var file = File(render, "image/png", $"{submissionId}.png");

        return file;
    }

    /// <summary>
    /// Delete the Submission with the given submissionId
    /// </summary>
    [HttpDelete("submissions/{submisisonId}")]
    public async Task<ActionResult> DeleteSubmission(string submisisonId)
    {
        if (Request.Headers["Bot_Secret"] != _settings.GetBotSecret()) return Unauthorized();

        var (isSuccessful,place) = await _memePlaceRepository.DeleteSubmission(submisisonId);
        if (!isSuccessful) 
            return NotFound("Cannot find submission with provided Id: " + submisisonId);

        var success = await _memePlaceRepository.RerenderPlace(place);
        if (!success) Console.WriteLine($"Failed to rerender place with Id: {place.Id}");

        return NoContent();
    }

    /// <summary>
    /// Force the service rerender the place with the given place Id
    /// </summary>
    [HttpPost("{placeId}/rerender")]
    public async Task<ActionResult> RerenderPlace(string placeId)
    {
        if (Request.Headers["Bot_Secret"] != _settings.GetBotSecret()) return Unauthorized();

        var place = await _memePlaceRepository.GetMemePlace(placeId);
        if (place == null) return NotFound("Cannot find place with provided Id: " + placeId);

        var success = await _memePlaceRepository.RerenderPlace(place);
        if (!success) Console.WriteLine($"Failed to rerender place with Id: {placeId}");

        return Ok();
    }

    /// <summary>
    /// Post a PlaceSubmission
    /// </summary>
    [HttpPost("submissions/submit")]
    public async Task<ActionResult<IEnumerable<PlaceSubmissionDTO>>> Submit([FromForm]PlaceSubmissionCreationDTO submissionDTO)
    {
        if (submissionDTO.ImageWithChanges.Length > 5000000)
            return StatusCode(413);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userRepository.GetUser(userId, includeDubloons: true);
        if (user == null) return NotFound(userId);

        var place = await _memePlaceRepository.GetMemePlace(submissionDTO.PlaceId);
        if (place == null) return NotFound(submissionDTO.PlaceId);

        var isBasedOnLatestRender = submissionDTO.ImageWithChanges.IsBasedOnLatestRender(place);
        if (!isBasedOnLatestRender) return BadRequest("The file is not based on the latest render of the given place. Please try again.");
        
        var latestRender = await _memePlaceRepository.GetLatestPlaceRender(submissionDTO.PlaceId);
        if (latestRender == null) return BadRequest();

        var changedPixels = submissionDTO.ImageWithChanges.ToSubmissionPixelChanges(latestRender);
        if (changedPixels.Count == 0)
            return BadRequest("The submission did not change any pixels. Please try again.");

        var requiredFunds = Math.Ceiling(changedPixels.Count / 100.0);
        var currentFunds = user.DubloonEvents.CountDubloons();

        if (currentFunds < requiredFunds)
            return BadRequest("Not enough dubloons to make submission. Dubloons needed: " + requiredFunds);

        var submission = await _memePlaceRepository.CreatePlaceSubmission(
            place, 
            user, 
            changedPixels, 
            submissionDTO.ImageWithChanges
        );

        var isSucessfulRender = await _memePlaceRepository.RenderDelta(place);

        if (!isSucessfulRender)
            Console.WriteLine("Failed To render new submission");

        return Ok(submission.ToPlaceSubmissionDTO());

    }
} 
