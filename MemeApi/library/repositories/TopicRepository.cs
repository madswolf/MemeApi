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

    public async Task<List<Topic>> GetTopicsByNameForUser(IEnumerable<string>? topicNames, string? userId = null)
    {
        return await GetTopicsByNameOrDefault(topicNames, userId);
    }

    public async Task<List<Topic>> GetTopicsByNameOrDefault(IEnumerable<string>? topicNames, string? userId = null)
    {
        if(topicNames != null)
        {
            var topics = await _context.Topics
                .Include(t => t.Owner)
                .Include(t => t.Moderators)
                .Where(t => topicNames.Contains(t.Name)).ToListAsync();

            var filteredTopics = topics.Where(t => t.CanUserPost(userId)).ToList();
            if (filteredTopics.Count == 0) return [await _context.Topics.FirstAsync(t => t.Name == _settings.GetDefaultTopicName())];
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
}
