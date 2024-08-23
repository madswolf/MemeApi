using MemeApi.library.Extensions;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemeApi.library.repositories;

public class TextRepository(MemeContext context, TopicRepository topicRepository, UserRepository userRepository)
{
    private readonly MemeContext _context = context;
    private readonly TopicRepository _topicRepository = topicRepository;
    private readonly UserRepository _userRepository = userRepository;

    public async Task<List<MemeText>> GetTexts(MemeTextPosition? type = null)
    {
        var texts = _context.Texts.Include(x => x.Votes).Include(t => t.Topics);
        if (type != null)
        {
            return await texts.Where(x => x.Position == type).ToListAsync();
        }

        return await texts.ToListAsync();
    }

    public async Task<MemeText?> GetText(string id)
    {
        return await _context.Texts.Include(x => x.Votes).Include(t => t.Topics).FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<MemeText> GetTextByContent(string content, MemeTextPosition position)
    {
        var existingText = await _context.Texts.FirstOrDefaultAsync(t => t.Text == content);
        if (existingText != null) return existingText;
        return new MemeText
        {
            Id = Guid.NewGuid().ToString(),
            Text = content,
            Position = position
        };
    }

    public MemeText GetRandomTextByType(MemeTextPosition type, string seed = "")
    {
        return _context.Texts.Where(x => x.Position == type).RandomItem(seed);
    }

    public MemeText GetRandomTextByTypeInTopic(MemeTextPosition type, Topic topic, string seed = "")
    {
        var list = _context.Texts.Include(t => t.Topics).Where(t => t.Topics.Contains(topic));
        return _context.Texts.Include(t => t.Topics).Where(t => t.Topics.Contains(topic)).Where(x => x.Position == type).RandomItem(seed);
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
        var user = await _userRepository.GetUser(userId);
        return await CreateText(text, position, topicNames, user);
    }

    public async Task<MemeText> CreateText(string text, MemeTextPosition position, IEnumerable<string>? topicNames = null, User? user = null)
    {
        var topics = await _topicRepository.GetTopicsByNameForUser(topicNames, user?.Id);

        var memeText = new MemeText
        {
            Id = Guid.NewGuid().ToString(),
            Topics = topics,
            Text = text,
            Position = position,
        };

        if (user != null) memeText.Owner = user;

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
        return _context.Texts.Any(e => e.Id == id);
    }
}
