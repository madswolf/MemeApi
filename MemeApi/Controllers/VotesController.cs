using MemeApi.library;
using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MemeApi.Controllers;

/// <summary>
/// A controller for creating managing votes
/// </summary>
[Route("[controller]")]
[ApiController]
public class VotesController : ControllerBase
{
    private readonly VotableRepository _votableRepository;
    private readonly TextRepository _textRepository;
    private readonly VisualRepository _visualRepository;
    private readonly MemeRepository _memeRepository;
    private readonly UserRepository _userRepository;
    private readonly MemeApiSettings _settings;
    /// <summary>
    /// A controller for creating managing votes
    /// </summary>
    public VotesController(VotableRepository votableRepository, TextRepository textRepository, VisualRepository visualRepository, MemeRepository memeRepository, UserRepository userRepository, MemeApiSettings settings)
    {
        _votableRepository = votableRepository;
        _textRepository = textRepository;
        _visualRepository = visualRepository;
        _memeRepository = memeRepository;
        _userRepository = userRepository;
        _settings = settings;
    }

    /// <summary>
    /// Get all votes.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Vote>>> GetVotes()
    {
        return await _votableRepository.GetVotes();
    }

    /// <summary>
    /// Get one vote.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Vote>> GetVote(string id)
    {
        var vote = await _votableRepository.GetVote(id);

        if (vote == null)
        {
            return NotFound();
        }

        return vote;
    }

    /// <summary>
    /// Vote on a particular meme component or meme component.
    /// To vote on a component or existing meme include one id, the id of the component.
    /// To vote for a meme that does not exist yet, include the id's of elements it contains.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<VoteDTO>> PostVote([FromForm]PostVoteDTO voteDTO)
    {
        if ((voteDTO.UpVote == null && voteDTO.VoteNumber == null) || 
            (voteDTO.UpVote != null && voteDTO.VoteNumber != null)) 
            return BadRequest("Supply either an Upvote or VoteNumber value");


        if (voteDTO.VoteNumber == null) voteDTO.VoteNumber = voteDTO.UpVote == Upvote.Upvote ? 9 : 0;
        if (voteDTO.UpVote == null) voteDTO.UpVote = voteDTO.VoteNumber < 5 ? Upvote.Downvote : Upvote.Upvote;

        var components = await _votableRepository.FindMany(voteDTO.ElementIDs);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if ((userId == null && voteDTO.ExternalUserID == null) || components.Count == 0)
        {
            return NotFound();
        }

        User? user = null;
        if (Request.Headers["Bot_Secret"] == _settings.GetBotSecret())
        {
            if (voteDTO.ExternalUserID == null || voteDTO.ExternalUserName == null) return BadRequest("Please include an external user whe voting on behalf of someone else");

            userId = voteDTO.ExternalUserID?.ToGuid();
            user = await _userRepository.GetUser(userId);
            if (user == null) {
                user = new User()
                {
                    Id = userId,
                    UserName = voteDTO.ExternalUserName,
                    ProfilePicFile = "default.jpg",
                    LastLoginAt = DateTime.UtcNow,
                };
            }
        }
        else
        {
            user = await _userRepository.GetUser(userId);
        }

        if (user == null) return NotFound("User not found");

        Votable element;

        if (components.Count > 1)
        {
            var visual = await _visualRepository.GetVisual(
                components.SingleOrDefault(item => item is MemeVisual)?.Id
                );
            if(visual == null) { return NotFound("Visual in meme is not present"); }

            var texts = await Task.WhenAll(
                components.Where(item => item is MemeText)
                    .Select(async text => await _textRepository.GetText(text.Id))
            );

            if (texts.Length == 0) return NotFound();
            var topText = texts.Length > 0 ? texts[0] : null;
            var bottomText = texts.Length > 1 ? texts[1] : null;

            element = await _memeRepository.UpsertByComponents(visual, topText, bottomText);
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
                return Ok(await _votableRepository.ChangeVote(existingVote, (Upvote)voteDTO.UpVote, (int)voteDTO.VoteNumber));
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
                Id = Guid.NewGuid().ToString(),
                Upvote = voteDTO.UpVote == Upvote.Upvote,
                Element = element,
                User = user,
                VoteNumber = (int)voteDTO.VoteNumber,
            };

            await _votableRepository.CreateVote(vote);
        }

        return CreatedAtAction("GetVote", new { id = vote.Id }, vote.ToVoteDTO());
    }

    /// <summary>
    /// Delete a vote. This can also be done with normal vote with Upvote = null
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteVote(string id)
    {
        var deleted = await _votableRepository.DeleteVote(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
