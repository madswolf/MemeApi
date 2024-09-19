using MemeApi.library;
using MemeApi.library.Extensions;
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
public class MemePlaceController : ControllerBase
{
    private readonly MemeApiSettings _settings;
    /// <summary>
    /// A controller for creating and managing visual meme components.
    /// </summary>
    public MemePlaceController( MemeApiSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Get all visuals
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemePlace>>> GetMemePlaces()
    {
        return Ok(visuals.Select(v => v.ToVisualDTO(_settings.GetMediaHost())));
    }
}
