using MemeApi.Models;
using MemeApi.Models.Context;
using MemeApi.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MemeApi.library.repositories;
using MemeApi.Models.Entity;

namespace MemeApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class VotesController : ControllerBase
    {
        private readonly VotableRepository _votableRepository;
        private readonly TextRepository _textRepository;
        private readonly VisualRepository _visualRepository;
        private readonly MemeRepository _memeRepository;
        private readonly UserRepository _userRepository;

        public VotesController(VotableRepository votableRepository, TextRepository textRepository, VisualRepository visualRepository, MemeRepository memeRepository, UserRepository userRepository)
        {
            _votableRepository = votableRepository;
            _textRepository = textRepository;
            _visualRepository = visualRepository;
            _memeRepository = memeRepository;
            _userRepository = userRepository;
        }

        // GET: api/Votes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vote>>> GetVotes()
        {
            return await _votableRepository.GetVotes();
        }

        // GET: api/Votes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Vote>> GetVote(int id)
        {
            var vote = await _votableRepository.GetVote(id);

            if (vote == null)
            {
                return NotFound();
            }

            return vote;
        }

        // POST: api/Votes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Vote>> PostVote([FromForm]VoteDTO voteDTO)
        {
            var components = await _votableRepository.FindMany(voteDTO.ElementIDs);
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null || components.Count == 0)
            {
                return NotFound();
            }

            var userId = int.Parse(userIdString);
            Votable element;

            if (components.Count > 1)
            {
                var visual = await _visualRepository.GetVisual(components.SingleOrDefault(item => item is MemeVisual).Id);
                var texts = await Task.WhenAll(
                    components.Where(item => item is MemeText)
                        .Select(async text => await _textRepository.GetText(text.Id))
                );

                if (texts.Length == 0) return NotFound();
                var toptext = texts.Length > 0 ? texts[0] : null;
                var bottomtext = texts.Length > 1 ? texts[1] : null; 
                
                var meme = await _memeRepository.FindByComponents(visual, toptext, bottomtext);
                if (meme == null)
                {
                    meme = new Meme
                    {
                        MemeVisual = visual,
                        //TODO handle which position they are in in the rendered meme when
                        Toptext = toptext,
                        BottomText = bottomtext
                    };

                    await _memeRepository.CreateMemeRaw(meme);
                }

                element = meme;
            }
            else
            {
                element = components[0];
            }

            Vote vote;
            var existingVote = _votableRepository.FindByElementAndUser(element, userId);

            if (existingVote != null)
            {
                if (voteDTO.UpVote != Upvote.Unvote)
                { 
                    existingVote.Upvote = voteDTO.UpVote == Upvote.Upvote;
                    vote = existingVote;
                }
                else
                {
                    return await DeleteVote(existingVote.Id);
                }
            }
            else
            {
                if (voteDTO.UpVote == Upvote.Unvote)
                {
                    return BadRequest("Can't unvote because no vote exists");
                }
                vote = new Vote
                {
                    Upvote = voteDTO.UpVote == Upvote.Upvote,
                    Element = element,
                    User = await _userRepository.GetUser(userId)
                };

                await _votableRepository.CreateVote(vote);
            }

            return CreatedAtAction("GetVote", new { id = vote.Id }, vote);
        }

        // DELETE: api/Votes/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteVote(int id)
        {
            var deleted = await _votableRepository.DeleteVote(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
