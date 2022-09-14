using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemeApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SoundsController : ControllerBase
    {
        private readonly MemeContext _context;

        public SoundsController(MemeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemeSound>>> GetSounds()
        {
            return await _context.Sounds.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MemeSound>> GetMemeSound(int id)
        {
            var memeSound = await _context.Sounds.FindAsync(id);

            if (memeSound == null)
            {
                return NotFound();
            }

            return memeSound;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMemeSound(int id, MemeSound memeSound)
        {
            if (id != memeSound.Id)
            {
                return BadRequest();
            }

            _context.Entry(memeSound).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemeSoundExists(id))
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

        [HttpPost]
        public async Task<ActionResult<MemeSound>> PostMemeSound(MemeSound memeSound)
        {
            _context.Sounds.Add(memeSound);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMemeSound", new { id = memeSound.Id }, memeSound);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMemeSound(int id)
        {
            var memeSound = await _context.Sounds.FindAsync(id);
            if (memeSound == null)
            {
                return NotFound();
            }

            _context.Sounds.Remove(memeSound);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MemeSoundExists(int id)
        {
            return _context.Sounds.Any(e => e.Id == id);
        }
    }
}
