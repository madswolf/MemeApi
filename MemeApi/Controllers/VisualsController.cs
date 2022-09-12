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
using AutoMapper;
using MemeApi.library;
using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.Models.DTO;
using Microsoft.AspNetCore.Http;

namespace MemeApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class VisualsController : ControllerBase
    {
        private readonly VisualRepository _visualRepository;
        private readonly IMapper _mapper;
        public VisualsController(VisualRepository visualRepository, IMapper mapper)
        {
            _visualRepository = visualRepository;
            _mapper = mapper;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemeVisual>>> GetVisuals() => await _visualRepository.GetVisuals();

            [HttpGet("{id}")]
        public async Task<ActionResult<MemeVisual>> GetMemeVisual(int id)
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
        public async Task<IActionResult> DeleteMemeVisual(int id)
        {
            var deleted = await _visualRepository.RemoveMemeVisual(id);
            if (deleted == false)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet]
        [Route("random")]
        public async Task<ActionResult<Meme>> RandomMeme()
        {
            var visual = (await _visualRepository.GetVisuals()).RandomItem();
            return Ok(_mapper.Map<MemeVisual, RandomComponentDTO>(visual));
        }
    }
}
