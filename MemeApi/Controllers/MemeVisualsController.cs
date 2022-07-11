using System;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MemeApi.library;
using MemeApi.library.repositories;
using Microsoft.AspNetCore.Http;

namespace MemeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemeVisualsController : ControllerBase
    {
        private readonly VisualRepository _visualRepository;
        public MemeVisualsController(VisualRepository visualRepository)
        {
            _visualRepository = visualRepository;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemeVisual>>> GetVisuals() => await _visualRepository.GetVisuals();

            [HttpGet("{id}")]
        public async Task<ActionResult<MemeVisual>> GetMemeVisual(long id)
        {
            var memeVisual = await _visualRepository.GetVisual(id);

            if (memeVisual == null) return NotFound();
            return memeVisual;
        }

        [HttpPost]
        public async Task<ActionResult<MemeVisual>> PostMemeVisual(IFormFile visual)
        {
            if (visual.Length > 5000) return StatusCode(413);

            var memeVisual = await _visualRepository.CreateMemeVisual(visual);
            return CreatedAtAction("GetMemeVisual", new { id = memeVisual.Id }, memeVisual);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMemeVisual(long id)
        {
            var deleted = await _visualRepository.RemoveMemeVisual(id);
            if (deleted == false)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
