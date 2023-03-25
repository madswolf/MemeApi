using System;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MemeApi.library.repositories;
using MemeApi.Models.DTO;
using System.ComponentModel;

namespace MemeApi.Controllers
{
    /// <summary>
    /// A controller for creating and managing meme and meme component groupings called topics.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TopicsController : ControllerBase
    {
        private readonly MemeContext _context;
        private readonly UserRepository _userRepository;
        /// <summary>
        /// A controller for creating and managing meme and meme component groupings called topics.
        /// </summary>
        public TopicsController(MemeContext context, UserRepository userRepository)
        {
            _context = context;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get all topics
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Topic>>> GetTopic()
        {
            return await _context.Topics.ToListAsync();
        }

        /// <summary>
        /// Get a specific topic by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Topic>> GetTopic(int id)
        {
            var topic = await _context.Topics.FindAsync(id);

            if (topic == null)
            {
                return NotFound();
            }

            return topic;
        }

        /// <summary>
        /// Mod a user for the given topic. Requires the currently logged in user to be the topic owner
        /// </summary>
        [HttpPut]
        [Route("[controller]/{topicId}/mod/{userId}")]
        public async Task<IActionResult> ModUser(int topicId, int userId)
        {
            var topic = await _context.Topics.FindAsync(topicId);
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (topic == null || ownerId == null)
            {
                return NotFound();
            }

            if (topic.Owner.Id != int.Parse(ownerId)) return Unauthorized();

            var userToMod = await _context.Users.FindAsync(userId);

            if(userToMod == null) return NotFound();

            topic.Moderators.Add(userToMod);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TopicExists(topicId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }

        /// <summary>
        /// Create a topic with the currently logged in user as the owner
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Topic>> CreateTopic(TopicCreationDTO topicCreationDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();
            var user = await _userRepository.GetUser(int.Parse(userId));

            var topic = new Topic()
            {
                Owner = user,
                Name = topicCreationDTO.TopicName,
                Description = topicCreationDTO.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

             _context.Topics.Add(topic);
            await _context.SaveChangesAsync();

            topic.Owner = null;
            return CreatedAtAction("GetTopic", new { id = topic.Id }, topic);
        }
        /// <summary>
        /// Delete Topic
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTopic(int id)
        {
            var topic = await _context.Topics.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (topic == null || userId == null)
            {
                return NotFound();
            }

            if (topic.Owner.Id != int.Parse(userId)) return Unauthorized();

            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        /// <summary>
        ///Delete a votable in a topic that you own or moderate
        /// </summary>
        [HttpDelete]
        [Route("[controller]/{id}")]
        public async Task<IActionResult> DeleteVotable(int id)
        {
            var votable = await _context.Votables.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (votable == null || userId == null) return NotFound();

            if (votable.Topic.Owner.Id != int.Parse(userId) || votable.Topic.Moderators.All(x => x.Id != int.Parse(userId)))
                return Unauthorized();

            _context.Votables.Remove(votable);
            return Ok();
        }

        private bool TopicExists(int id)
        {
            return _context.Topics.Any(e => e.Id == id);
        }
    }
}
