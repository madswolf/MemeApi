using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MemeApi.library.Extensions;
using MemeApi.library.Services.Files;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MemeApi.library.repositories;

public class VisualRepository
{
    private readonly MemeContext _context;
    private readonly TopicRepository _topicRepository;
    private readonly IFileSaver _fileSaver;
    private readonly IFileRemover _fileRemover;
    private static readonly Random Random = Random.Shared;
    public VisualRepository(MemeContext context, IFileSaver fileSaver, IFileRemover fileRemover, TopicRepository topicRepository)
    {
        _context = context;
        _fileSaver = fileSaver;
        _fileRemover = fileRemover;
        _topicRepository = topicRepository;
    }

    public async Task<List<MemeVisual>> GetVisuals()
    {
        return await _context.Visuals
            .Include(x => x.Votes)
            .Include(x => x.Topics)
            .ToListAsync();
    }

    public MemeVisual GetRandomVisual(string seed = "")
    {
        var regex = new Regex("^.*\\.gif$");
        return _context.Visuals.ToList().Where(x => !regex.IsMatch(x.Filename)).ToList().RandomItem(seed);
    }

    public async Task<MemeVisual?> GetVisual(string? id)
    {
        return await _context.Visuals.Include(x => x.Votes).FirstOrDefaultAsync(v => v.Id == id);
    }

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }

    public async Task<MemeVisual> CreateMemeVisual(IFormFile visual, string filename, IEnumerable<string>? topicNames = null, string? userId = null)
    {
        var defaultTopic = _context.Topics.FirstOrDefault(t => t.Name == "Rotte-Grotte");
        if (defaultTopic == null)
        {
            throw new Exception("Default topic 'Swu-legacy' not found");
        }

        // Create a new Votable entity
        var votable = new Votable
        {
            Id = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            Topics = new List<Topic> { defaultTopic },
            Votes = new List<Vote>()
        };

        // Add the new Votable to the context
        _context.Votables.Add(votable);
        
        var changess = _context.ChangeTracker.Entries();
        // Save changes to the database
        _context.SaveChanges();
        
        var topics = await _topicRepository.GetTopicsByNameForUser(topicNames, userId);

        foreach (var topic in topics)
        {
            _context.Entry(topic).State = EntityState.Unchanged;
        }
        
        votable = new Votable
        {
            Id = Guid.NewGuid().ToString(),
            Topics = topics.ToList(),
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
        };

        var memeVisual = new MemeVisual()
        {
            Id = Guid.NewGuid().ToString(),
            Filename = filename
        };

        if (_context.Visuals.Any(x => x.Filename == memeVisual.Filename))
        {
            memeVisual.Filename = RandomString(5) + memeVisual.Filename;
        }

        await _fileSaver.SaveFile(visual, "visual/", memeVisual.Filename);

        _context.Visuals.Add(memeVisual);
        var changes = _context.ChangeTracker.Entries();
        await _context.SaveChangesAsync();
        return memeVisual;
    }

    public async Task<bool> Delete(string id)
    {
        var memeVisual = await _context.Visuals.FindAsync(id);
        if (memeVisual == null)
        {
            return false;
        }

        await _fileRemover.RemoveFile(Path.Combine("visual/", memeVisual.Filename));

        _context.Visuals.Remove(memeVisual);
        await _context.SaveChangesAsync();

        return true;
    }
}
