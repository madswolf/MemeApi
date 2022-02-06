using System;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MemeApi.library;
using Microsoft.AspNetCore.Http;

namespace MemeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemeVisualsController : ControllerBase
    {
        private readonly MemeContext _context;
        private readonly IFileSaver _fileSaver;
        private readonly IFileRemover _fileRemover;
        private static readonly Random _random = new();

        public MemeVisualsController(MemeContext context, IFileSaver fileSaver, IFileRemover fileRemover)
        {
            _context = context;
            _fileSaver = fileSaver;
            _fileRemover = fileRemover;
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
        // POST: api/MemeVisuals
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MemeVisual>> PostMemeVisual(IFormFile visual)
        {
            if (visual.Length > 5000)
            {
                return StatusCode(413);
            }

            var memeVisual = new MemeVisual()
            {
                Filename = visual.FileName
            };

            if (_context.Visuals.Any(x => x.Filename == visual.FileName))
            {
                memeVisual.Filename = RandomString(5) + memeVisual.Filename;
            }

            _fileSaver.SaveFile(visual, "uploads/");
            
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

            _fileRemover.RemoveFile(Path.Combine("uploads/", memeVisual.Filename));

            _context.Visuals.Remove(memeVisual);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MemeVisualExists(long id)
        {
            return _context.Visuals.Any(e => e.Id == id);
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}
