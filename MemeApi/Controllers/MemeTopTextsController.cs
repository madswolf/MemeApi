using MemeApi.Models;
using MemeApi.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemeTopTextsController : ControllerBase
    {
        private readonly MemeContext _context;

        public MemeTopTextsController(MemeContext context)
        {
            _context = context;
        }

        // GET: api/MemeTopTexts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemeTopText>>> GetToptexts()
        {
            return await _context.Toptexts.ToListAsync();
        }

        // GET: api/MemeTopTexts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MemeTopText>> GetMemeTopText(long id)
        {
            var memeTopText = await _context.Toptexts.FindAsync(id);

            if (memeTopText == null)
            {
                return NotFound();
            }

            return memeTopText;
        }

        // PUT: api/MemeTopTexts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMemeTopText(long id, MemeTopText memeTopText)
        {
            if (id != memeTopText.Id)
            {
                return BadRequest();
            }

            _context.Entry(memeTopText).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemeTopTextExists(id))
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

        // POST: api/MemeTopTexts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MemeTopText>> PostMemeTopText(MemeTopText memeTopText)
        {
            _context.Toptexts.Add(memeTopText);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMemeTopText", new { id = memeTopText.Id }, memeTopText);
        }

        // DELETE: api/MemeTopTexts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMemeTopText(long id)
        {
            var memeTopText = await _context.Toptexts.FindAsync(id);
            if (memeTopText == null)
            {
                return NotFound();
            }

            _context.Toptexts.Remove(memeTopText);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MemeTopTextExists(long id)
        {
            return _context.Toptexts.Any(e => e.Id == id);
        }
    }
}
