using System;
using MemeApi.Models;
using MemeApi.Models.Context;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemesController : ControllerBase
    {
        private readonly MemeContext _context;

        public MemesController(MemeContext context)
        {
            _context = context;
        }

        // GET: api/Memes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Meme>>> GetMemes()
        {
            return await _context.Memes
                .Include(m => m.MemeVisual)
                .Include(m => m.MemeSound)
                .Include(m => m.MemeTexts)
                .ToListAsync();
        }

        // GET: api/Memes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Meme>> GetMeme(long id)
        {
            var meme = _context.Memes
                .Include(m => m.MemeVisual)
                .Include(m => m.MemeSound)
                .Include(m => m.MemeTexts)
                .First(m => m.Id == id);

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

            _context.Entry(meme).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Memes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Meme>> PostMeme(MemeCreationDTO memeDTO)
        {
            var memeVisual = new MemeVisual { Filename = memeDTO.VisualFile };

            _context.Visuals.Add(memeVisual);

            var meme = new Meme
            {
                MemeVisual = memeVisual,
            };

            if (memeDTO.SoundFile != null)
            {
                var memeSound = new MemeSound { Filename = memeDTO.SoundFile };
                _context.Sounds.Add(memeSound);
                meme.MemeSound = memeSound;
            }

            if (memeDTO.Texts != null)
            {
                var memeTexts = memeDTO.Texts.Select(item =>
                {
                    var (text, position) = item;
                    var memeText = new MemeText
                    {
                        Memetext = text,
                        postion = (MemeTextPosition)Enum.Parse(typeof(MemeTextPosition), position)
                    };
                    _context.Texts.Add(memeText);
                    return memeText;
                });
                meme.MemeTexts = memeTexts.ToList();
            }

            _context.Memes.Add(meme);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMeme), new { id = meme.Id }, meme);
        }

        // DELETE: api/Memes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeme(long id)
        {
            var meme = await _context.Memes.FindAsync(id);
            if (meme == null)
            {
                return NotFound();
            }

            _context.Memes.Remove(meme);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MemeExists(long id)
        {
            return _context.Memes.Any(e => e.Id == id);
        }
    }
}

