using MemeApi.library.Extensions;
using MemeApi.Models.Context;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using MemeApi.Models.Entity.Memes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemeApi.library.repositories;

public class TopicRepository
{
    private readonly MemeContext _context;
    private readonly UserRepository _userRepository;
    private readonly MemeApiSettings _settings;

    public TopicRepository(MemeContext context, UserRepository userRepository, MemeApiSettings settings)
    {
        _context = context;
        _userRepository = userRepository;
        _settings = settings;
    }

    public async Task<List<TopicDTO>> GetTopics()
    {
        var topics = await _context.Topics
            .Include(t => t.Owner)
            .Include(t => t.Moderators)
            .Select(t => t.ToTopicDTO())
            .ToListAsync();

        return topics;
    }

    public async Task<Topic?> GetTopic(string id)
    {
        return await _context.Topics.Include(t => t.Moderators).FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Topic?> GetTopicByName(string? name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        return await _context.Topics.Where(t => t.Name == name).FirstOrDefaultAsync();
    }

    public async Task<List<Topic>> GetTopicsByNameForUser(IEnumerable<string>? topicNames, string? userId = null, bool includeDefault = true)
    {
        if(topicNames != null)
        {
            var topics = await _context.Topics
                .Include(t => t.Owner)
                .Include(t => t.Moderators)
                .Where(t => topicNames.Contains(t.Name)).ToListAsync();

            var filteredTopics = topics.Where(t => t.CanUserPost(userId)).ToList();
            if (filteredTopics.Count == 0 && includeDefault) return [await _context.Topics.FirstAsync(t => t.Name == _settings.GetDefaultTopicName())];
            return filteredTopics;
        }

        return [await _context.Topics.FirstAsync(t => t.Name == _settings.GetDefaultTopicName())];
    }
    public async Task<Topic> GetDefaultTopic()
    {
        return await _context.Topics.FirstAsync(t => t.Name == _settings.GetDefaultTopicName());
    }

    public async Task<bool> UpdateTopic(string id, string? name = null, string? description = null)
    {
        var topic = await _context.Topics.FindAsync(id);

        if (topic == null)
        {
            return false;
        }

        if (name == null && description == null) return false;

        if(name != null)
        {
            topic.Name = name;
        }

        if (description != null)
        {
            topic.Description = description;
        }

        try
        {
            topic.LastUpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TopicExists(id))
            {
                return false;
            }
            else
            {
                throw;
            }
        }

        return true;
    }

    public async Task<TopicDTO?> CreateTopic(TopicCreationDTO topicCreationDTO, string userId)
    {
        var user = await _userRepository.GetUser(userId);
        if (user == null) return null;

        var topic = new Topic()
        {
            Id = Guid.NewGuid().ToString(),
            Owner = user,
            Name = topicCreationDTO.TopicName,
            Description = topicCreationDTO.Description,
            Moderators = [],
            HasRestrictedPosting = topicCreationDTO != null && topicCreationDTO.HasRestrictedPosting,
        };

        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();
        return topic.ToTopicDTO();
    }

    public async Task<bool> DeleteTopic(Topic topic, User user)
    {
        if (topic.Owner != user) return false;

        _context.Topics.Remove(topic);
        await _context.SaveChangesAsync();

        return true;
    }

    private bool TopicExists(string id)
    {
        return _context.Topics.Any(e => e.Id == id);
    }

    internal async Task<bool> ModUser(Topic topic, string userId)
    {
        var user = await _userRepository.GetUser(userId);
        if (user == null) return false;
        if(topic.Moderators.Any(t => t.Id == userId) || topic.OwnerId == userId) return false;
        topic.Moderators.Add(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(T?, List<string>?)> GetOrUpdateVotableIfExistsAndFilterTopics<T>(string contentHash, User? user, List<string>? topicNames) where T : Votable, new()
    {
        var votables = (await GetIfExists(contentHash)).OrderBy(v => v.CreatedAt).ToList();
        if (votables.Count == 0) return (null, topicNames);

        var topics = votables.SelectMany(v => v.Topics).GroupBy(t => t.Id).Select(g => g.First()).ToList();
        var topicNamesWithoutContent = topicNames?.Where(t => !topics.Exists(topic => topic.Name == t)).ToList();

        if (topicNamesWithoutContent == null || topicNamesWithoutContent.Count == 0) return (votables.First().ToVotableOfType<T>(), []);

        var userOwnedVotable = votables.FirstOrDefault(v => v.OwnerId == user?.Id);
        if (userOwnedVotable == null || user == null) return (null, topicNamesWithoutContent); // remember to filter topics that already have given content

        var topicsWithoutContent = await GetTopicsByNameForUser(topicNamesWithoutContent, user.Id, includeDefault: false);
        userOwnedVotable.Topics.AddRange(topicsWithoutContent);
        return (userOwnedVotable.ToVotableOfType<T>(), []);
    }
    
    public async Task<List<Votable>> GetIfExists(string contentHash)
    {
        return await _context.Votables
            .Include(v => v.Owner)
            .Include(v => v.Topics)
            .Where(v => v.ContentHash == contentHash)
            .ToListAsync();
    }
}
