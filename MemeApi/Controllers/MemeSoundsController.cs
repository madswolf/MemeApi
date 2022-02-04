using MemeApi.Models.Context;
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
    public class MemeSoundsController : ControllerBase
    {
        private readonly MemeContext _context;

        public MemeSoundsController(MemeContext context)
        {
            _context = context;
        }

        // GET: api/MemeSounds
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemeSound>>> GetSounds()
        {
            return await _context.Sounds.ToListAsync();
        }

        // GET: api/MemeSounds/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MemeSound>> GetMemeSound(long id)
        {
            var memeSound = await _context.Sounds.FindAsync(id);

            if (memeSound == null)
            {
                return NotFound();
            }

            return memeSound;
        }

        // PUT: api/MemeSounds/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMemeSound(long id, MemeSound memeSound)
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

        // POST: api/MemeSounds
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MemeSound>> PostMemeSound(MemeSound memeSound)
        {
            _context.Sounds.Add(memeSound);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMemeSound", new { id = memeSound.Id }, memeSound);
        }

        // DELETE: api/MemeSounds/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMemeSound(long id)
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

        private bool MemeSoundExists(long id)
        {
            return _context.Sounds.Any(e => e.Id == id);
        }
    }
}
