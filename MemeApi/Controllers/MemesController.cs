using MemeApi.Models.Context;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemeApi.library.repositories;

namespace MemeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemesController : ControllerBase
    {
        private readonly MemeContext _context;
        private readonly MemeRepository _memeRepository;

        public MemesController(MemeContext context, MemeRepository memeRepository)
        {
            _context = context;
            _memeRepository = memeRepository;
        }

        // GET: api/Memes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Meme>>> GetMemes()
        {
            return await _memeRepository.GetMemes();
        }


        // GET: api/Memes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Meme>> GetMeme(long id)
        {
            var meme = _memeRepository.GetMeme(id);

            if (meme == null)
            {
                return NotFound();
            }

            return meme;
        }

        // PUT: api/Memes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMeme(long id, Meme meme)
        {
            if (id != meme.Id)
            {
                return BadRequest();
            }

            if (await _memeRepository.ModifyMeme(id, meme)) return NotFound();

            return NoContent();
        }

        // POST: api/Memes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Meme>> PostMeme(MemeCreationDTO memeDTO)
        {
            var meme = _memeRepository.CreateMeme(memeDTO);
            return CreatedAtAction(nameof(GetMeme), new { id = meme.Id }, meme);
        }

        // DELETE: api/Memes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeme(long id)
        {
            if (await _memeRepository.DeleteMeme(id)) return NotFound();

            return NoContent();
        }
    }
}

