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
    public class MemeBottomTextsController : ControllerBase
    {
        private readonly MemeContext _context;

        public MemeBottomTextsController(MemeContext context)
        {
            _context = context;
        }

        // GET: api/MemeBottomTexts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemeBottomText>>> GetBottomTexts()
        {
            return await _context.BottomTexts.ToListAsync();
        }

        // GET: api/MemeBottomTexts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MemeBottomText>> GetMemeBottomText(long id)
        {
            var memeBottomText = await _context.BottomTexts.FindAsync(id);

            if (memeBottomText == null)
            {
                return NotFound();
            }

            return memeBottomText;
        }

        // PUT: api/MemeBottomTexts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMemeBottomText(long id, MemeBottomText memeBottomText)
        {
            if (id != memeBottomText.Id)
            {
                return BadRequest();
            }

            _context.Entry(memeBottomText).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemeBottomTextExists(id))
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

        // POST: api/MemeBottomTexts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MemeBottomText>> PostMemeBottomText(MemeBottomText memeBottomText)
        {
            _context.BottomTexts.Add(memeBottomText);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMemeBottomText", new { id = memeBottomText.Id }, memeBottomText);
        }

        // DELETE: api/MemeBottomTexts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMemeBottomText(long id)
        {
            var memeBottomText = await _context.BottomTexts.FindAsync(id);
            if (memeBottomText == null)
            {
                return NotFound();
            }

            _context.BottomTexts.Remove(memeBottomText);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MemeBottomTextExists(long id)
        {
            return _context.BottomTexts.Any(e => e.Id == id);
        }
    }
}
