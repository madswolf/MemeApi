using MemeApi.Models;
using MemeApi.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemeApi.Models.Entity;

namespace MemeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemeVisualsController : ControllerBase
    {
        private readonly MemeContext _context;

        public MemeVisualsController(MemeContext context)
        {
            _context = context;
        }

        // GET: api/MemeVisuals
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemeVisual>>> GetVisuals()
        {
            return await _context.Visuals.ToListAsync();
        }

        // GET: api/MemeVisuals/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MemeVisual>> GetMemeVisual(long id)
        {
            var memeVisual = await _context.Visuals.FindAsync(id);

            if (memeVisual == null)
            {
                return NotFound();
            }

            return memeVisual;
        }

        // PUT: api/MemeVisuals/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMemeVisual(long id, MemeVisual memeVisual)
        {
            if (id != memeVisual.Id)
            {
                return BadRequest();
            }

            _context.Entry(memeVisual).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemeVisualExists(id))
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

        // POST: api/MemeVisuals
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MemeVisual>> PostMemeVisual(MemeVisual memeVisual)
        {
            _context.Visuals.Add(memeVisual);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMemeVisual", new { id = memeVisual.Id }, memeVisual);
        }

        // DELETE: api/MemeVisuals/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMemeVisual(long id)
        {
            var memeVisual = await _context.Visuals.FindAsync(id);
            if (memeVisual == null)
            {
                return NotFound();
            }

            _context.Visuals.Remove(memeVisual);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MemeVisualExists(long id)
        {
            return _context.Visuals.Any(e => e.Id == id);
        }
    }
}
