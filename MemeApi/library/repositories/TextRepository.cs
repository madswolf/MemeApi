﻿using MemeApi.library.Extensions;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemeApi.library.repositories;

public class TextRepository(MemeContext context, TopicRepository topicRepository)
{
    private readonly MemeContext _context = context;
    private readonly TopicRepository _topicRepository = topicRepository;

    public async Task<List<MemeText>> GetTexts(MemeTextPosition? type = null)
    {
        var texts = _context.Texts.Include(x => x.Votable.Votes).Include(t => t.Votable.Topics);
        if (type != null)
        {
            return await texts.Where(x => x.Position == type).ToListAsync();
        }
        
        return await texts.ToListAsync();
    }

    public async Task<MemeText?> GetText(string id)
    {
        return await _context.Texts.Include(x => x.Votable.Votes).Include(t => t.Votable.Topics).FirstOrDefaultAsync(t => t.VotableId == id);
    }

    public async Task<MemeText> GetTextByContent(string content, MemeTextPosition position)
    {
        var existingText = await _context.Texts.FirstOrDefaultAsync(t => t.Text == content);
        if (existingText != null) return existingText;
        var votable = new Votable
        {
            Id = Guid.NewGuid().ToString(),
            Topics = [await _topicRepository.GetDefaultTopic()],
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };

        return new MemeText
        {
            VotableId = votable.Id,
            Votable = votable,
            Text = content,
            Position = position
        };
    }

    public MemeText GetRandomTextByType(MemeTextPosition type, string seed = "")
    {
        return _context.Texts.Where(x => x.Position == type).RandomItem(seed);
    }


    public MemeText GetRandomText(string seed = "")
    {
        return _context.Texts.RandomItem(seed);
    }

    public async Task<bool> UpdateText(string id, string newMemeBottomText, MemeTextPosition? newMemeTextPosition = null)
    {
        var memeText = await _context.Texts.FindAsync(id);

        if (memeText == null)
        {
            return false;
        }
        memeText.Text = newMemeBottomText;
        if (newMemeTextPosition != null)
        {
            memeText.Position = (MemeTextPosition)newMemeTextPosition;
        }

        try
        {
            memeText.Votable.LastUpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MemeBottomTextExists(id))
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

    public async Task<MemeText> CreateText(string text, MemeTextPosition position, IEnumerable<string>? topicNames = null, string? userId = null)
    {
        var topics = await _topicRepository.GetTopicsByNameForUser(topicNames, userId);
        var votable = new Votable
        {
            Id = Guid.NewGuid().ToString(),
            Topics = topics,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
        };

        var memeText = new MemeText
        {
            Votable = votable,
            VotableId = votable.Id,
            Text = text,
            Position = position,
        };

        _context.Texts.Add(memeText);
        await _context.SaveChangesAsync();
        return memeText;
    }

    public async Task<bool> Delete(string id)
    {
        var memeBottomText = await _context.Texts.FindAsync(id);
        if (memeBottomText == null) return false;
        

        _context.Texts.Remove(memeBottomText);
        await _context.SaveChangesAsync();

        return true;
    }

    private bool MemeBottomTextExists(string id)
    {
        return _context.Texts.Any(e => e.VotableId == id);
    }
}
