using MemeApi.library;
using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.library.Repositories;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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

    /// <summary>
    /// A controller for creating and managing visual meme components.
    /// </summary>
    public MemePlacesController(MemePlaceRepository memePlaceRepository, MemeApiSettings settings, UserRepository userRepository)
    {
        _memePlaceRepository = memePlaceRepository;
        _settings = settings;
        _userRepository = userRepository;
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
    [HttpGet("{placeId}/submisisons")]
    public async Task<ActionResult<IEnumerable<PlaceSubmissionDTO>>> GetPlaceSubmissions(string placeId)
    {
        var places = await _memePlaceRepository.GetMemePlaceSubmissions(placeId);
        return Ok(places.Select(p => p.ToPlaceSubmissionDTO()));
    }

    /// <summary>
    /// Post a PlaceSubmission
    /// </summary>
    [HttpPost("/submisisons/submit")]
    public async Task<ActionResult<IEnumerable<PlaceSubmissionDTO>>> GetPlaceSubmissions([FromForm]PlaceSubmissionCreationDTO submissionDTO)
    {
        if (submissionDTO.ImageWithChanges.Length > 5000000)
            return StatusCode(413);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userRepository.GetUser(userId, includeDubloons: true);
        if (user == null) return NotFound(userId);

        var place = await _memePlaceRepository.GetMemePlace(submissionDTO.PlaceId);
        if (place == null) return NotFound(submissionDTO.PlaceId);

        var renderedPlaceImage = place.ToRenderedPlaceImage();

        var submission = await _memePlaceRepository.CreatePlaceSubmission(place, user, submissionDTO.ImageWithChanges);
        return Ok(submission.ToPlaceSubmissionDTO());
    }
}
