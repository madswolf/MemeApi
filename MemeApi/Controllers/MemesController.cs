using System;
using MemeApi.library.repositories;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using MemeApi.library.Extensions;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Text.RegularExpressions;

namespace MemeApi.Controllers
{

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
        public async Task<ActionResult<IEnumerable<Meme>>> GetMemes()
        {
            return await _memeRepository.GetMemes();
        }

        /// <summary>
        /// Get a specific meme by ID
        /// </summary> 
        [HttpGet("{id}")]
        public async Task<ActionResult<Meme>> GetMeme(int id)
        {
            var meme = await _memeRepository.GetMeme(id);
            if (meme == null) return NotFound();
 
            return Ok(meme);
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
        public async Task<ActionResult<Meme>> PostMeme([FromForm]MemeCreationDTO memeCreationDto)
        {
            if (!memeCreationDto.VisualFile.FileName.Equals("VisualFile")) memeCreationDto.FileName = memeCreationDto.VisualFile.FileName;
            var meme = await _memeRepository.CreateMeme(memeCreationDto);
            return CreatedAtAction(nameof(GetMeme), new { id = meme.Id }, meme);
        }

        /// <summary>
        /// Delete a meme
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeme(int id)
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
}

