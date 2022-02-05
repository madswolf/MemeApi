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
    public class MemeTextsController : ControllerBase
    {
        private readonly MemeContext _context;

        public MemeTextsController(MemeContext context)
        {
            _context = context;
        }

        // GET: api/MemeBottomTexts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemeText>>> GetBottomTexts()
        {
            return await _context.Texts.ToListAsync();
        }

        // GET: api/MemeBottomTexts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MemeText>> GetMemeBottomText(long id)
        {
            var memeBottomText = await _context.Texts.FindAsync(id);

            if (memeBottomText == null)
            {
                return NotFound();
            }

            return Ok(memeBottomText);
        }

        // PUT: api/MemeBottomTexts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMemeText(long id, string newMemeBottomText, MemeTextPosition? newMemeTextPosition = null)
        {
            var memeText = await _context.Texts.FindAsync(id);

            if (memeText == null)
            {
                return NotFound();
            }
            memeText.Text = newMemeBottomText;
            if (newMemeTextPosition != null)
            {
                memeText.Position = (MemeTextPosition)newMemeTextPosition;
            }

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
        public async Task<ActionResult<MemeText>> CreateMemeBottomText(string text, MemeTextPosition position)
        {
            var memeText = new MemeText();
            memeText.Text = text;
            memeText.Position = position;
            _context.Texts.Add(memeText);
            await _context.SaveChangesAsync();

            return CreatedAtAction("CreateMemeBottomText", new { id = memeText.Id }, memeText);
        }

        // DELETE: api/MemeBottomTexts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMemeBottomText(long id)
        {
            var memeBottomText = await _context.Texts.FindAsync(id);
            if (memeBottomText == null)
            {
                return NotFound();
            }

            _context.Texts.Remove(memeBottomText);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MemeBottomTextExists(long id)
        {
            return _context.Texts.Any(e => e.Id == id);
        }
    }
}
