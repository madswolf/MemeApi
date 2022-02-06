using MemeApi.Models;
using MemeApi.Models.Context;
using MemeApi.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VotesController : ControllerBase
    {
        private readonly MemeContext _context;

        public VotesController(MemeContext context)
        {
            _context = context;
        }

        // GET: api/Votes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vote>>> GetVotes()
        {
            return await _context.Votes.ToListAsync();
        }

        // GET: api/Votes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Vote>> GetVote(long id)
        {
            var vote = await _context.Votes.FindAsync(id);

            if (vote == null)
            {
                return NotFound();
            }

            return vote;
        }

        // POST: api/Votes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Vote>> PostVote(VoteDTO voteDTO)
        {
            var user = await _context.Votables.FindAsync(voteDTO.ElementID);
            var element = await _context.Users.FindAsync(voteDTO.UserID);

            if(user == null || element == null)
            {
                return NotFound();
            }

            Vote vote;
            var existingVote = _context.Votes
                .Select(x => x)
                .Include(x => x.Element)
                .Include(x => x.User)
                .SingleOrDefault(x => x.Element.Id == element.Id && x.User.Id == user.Id);

            if (existingVote != null)
            {
                if (voteDTO.UpVote != null)
                { 
                    existingVote.Upvote = (bool)voteDTO.UpVote;
                    vote = existingVote;
                }
                else
                {
                    return await DeleteVote(existingVote.Id);
                }
            }
            else
            {
                if (voteDTO.UpVote == null)
                {
                    return StatusCode(400);
                }
                vote = new Vote
                {
                    Upvote = (bool)voteDTO.UpVote,
                    Element = user,
                    User = element
                };

                _context.Votes.Add(vote);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVote", new { id = vote.Id }, vote);
        }

        // DELETE: api/Votes/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteVote(long id)
        {
            var vote = await _context.Votes.FindAsync(id);
            if (vote == null)
            {
                return NotFound();
            }

            _context.Votes.Remove(vote);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VoteExists(long id)
        {
            return _context.Votes.Any(e => e.Id == id);
        }
    }
}
