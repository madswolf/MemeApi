using AutoMapper;
using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MemeApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class VisualsController : ControllerBase
    {
        private readonly VisualRepository _visualRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public VisualsController(VisualRepository visualRepository, IMapper mapper, IConfiguration configuration)
        {
            _visualRepository = visualRepository;
            _mapper = mapper;
            _configuration = configuration;
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

            var memeVisual = await _visualRepository.CreateMemeVisual(visual, visual.FileName);
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
        public async Task<ActionResult<RandomComponentDTO>> RandomMeme()
        {
            var visual = (await _visualRepository.GetVisuals()).RandomItem();
            var randomDTO = new RandomComponentDTO
            {
                data = _configuration["Media.Host"] + "visual/" + visual.Filename,
                id = visual.Id,
                votes = visual.SumVotes()
            };
            return Ok(randomDTO);
        }
    }
}
