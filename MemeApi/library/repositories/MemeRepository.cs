using MemeApi.library.Extensions;
using MemeApi.Models.Context;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemeApi.library.repositories;

public class MemeRepository
{
    private readonly MemeContext _context;
    private readonly VisualRepository _visualRepository;
    private readonly TextRepository _textRepository;
    private readonly TopicRepository _topicRepository;
    private readonly MemeApiSettings _settings;
    public MemeRepository(MemeContext context, VisualRepository visualRepository, TextRepository textRepository, TopicRepository topicRepository, MemeApiSettings settings)
    {
        _context = context;
        _visualRepository = visualRepository;
        _textRepository = textRepository;
        _topicRepository = topicRepository;
        _settings = settings;
    }

    public async Task<Meme?> CreateMeme(MemeCreationDTO memeDTO, string? userId = null)
    {
        var memeVisual = await _visualRepository.CreateMemeVisual(memeDTO.VisualFile, memeDTO.FileName, memeDTO.Topics, userId);

        var meme = new Meme
        {
            Id = Guid.NewGuid().ToString(),
            MemeVisual = memeVisual,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
        };

        //if (memeDTO.SoundFile != null)
        //{
        //    var memeSound = new MemeSound { Filename = memeDTO.SoundFile };
        //    _context.Sounds.Add(memeSound);
        //    meme.MemeSound = memeSound;
        //}

        if (memeDTO.TopText != null)
        {
            meme.TopText = await _textRepository.CreateText(memeDTO.TopText, MemeTextPosition.TopText, memeDTO.Topics, userId);
        }

        if (memeDTO.BottomText != null)
        {
            meme.BottomText = await _textRepository.CreateText(memeDTO.BottomText, MemeTextPosition.BottomText, memeDTO.Topics, userId);
        }

        var topics = await _topicRepository.GetTopicsByNameForUser(memeDTO.Topics, userId );

        if (topics.Any(t => t == null)) return null;

        meme.Topics = topics.ToList();

        _context.Memes.Add(meme);
        await _context.SaveChangesAsync();
        return meme;
    }

    public async Task<Meme> UpsertByComponents(MemeVisual visual, MemeText? toptext, MemeText? bottomtext, Topic? topic = null)
    {
        var meme = await FindByComponents(visual, toptext, bottomtext);
        if (topic == null) topic = await _topicRepository.GetDefaultTopic();
        if (meme == null)
        {
            meme = new Meme
            {
                Id = Guid.NewGuid().ToString(),
                MemeVisual = visual,
                //TODO handle which position they are in in the rendered meme when
                TopText = toptext,
                BottomText = bottomtext,
                CreatedAt = DateTime.UtcNow,
                Topics = [topic]
            };

            await CreateMemeRaw(meme);
        }
        else if(topic != null)
        {
            meme.Topics.Add(topic);
            await _context.SaveChangesAsync();
        }

        return meme;
    }

    public async Task<Meme> CreateMemeRaw(Meme meme)
    {
        await _context.Memes.AddAsync(meme);
        await _context.SaveChangesAsync();
        return meme;
    }
    public async Task<bool> DeleteMeme(string id)
    {
        var meme = await _context.Memes.FindAsync(id);
        if (meme == null)
        {
            return true;
        }

        _context.Memes.Remove(meme);
        await _context.SaveChangesAsync();
        return false;
    }

    public async Task<List<Meme>> GetMemes()
    {
        return await IncludeParts()
            .ToListAsync();
    }

    public async Task<Meme?> GetMeme(string id)
    {
        return await IncludeParts()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    private IIncludableQueryable<Meme, MemeText?> IncludeParts()
    {
        return _context.Memes
            .Include(m => m.MemeVisual)
            .Include(m => m.Topics)
            .Include(m => m.TopText)
            .Include(m => m.BottomText);
    }
    public async Task<Meme> RandomMemeByComponents(string? topText = null, string? bottomText = null, string? topicName = null)
    {
        var visual = _visualRepository.GetRandomVisual();
        var toptext = topText == null ? 
            _textRepository.GetRandomTextByType(MemeTextPosition.TopText) 
            : await _textRepository.GetTextByContent(topText, MemeTextPosition.TopText);

        var bottomtext = bottomText == null ?
            _textRepository.GetRandomTextByType(MemeTextPosition.BottomText)
            : await _textRepository.GetTextByContent(bottomText, MemeTextPosition.BottomText);

        var topic = topicName != null ? await _topicRepository.GetTopicByName(topicName) : null;

        var meme = await UpsertByComponents(visual, toptext, bottomtext, topic);
        return meme;
    }

    public bool HasMemeOfTheDay(DateTime date)
    {
        var memes = _context.Memes.
            Include(m => m.Topics)
            .Where(m =>
                m.Topics.Any(t => t.Name == _settings.GetMemeOfTheDayTopicName())
                ).ToList();
        return memes.Any(m => m.CreatedAt.Date == date.Date);
    }
    public async Task<Meme?> FindByComponents(MemeVisual visual, MemeText? toptext = null, MemeText? bottomtext = null)
    {
        var memes = _context.Memes
            .Include(meme => meme.MemeVisual)
            .Include(meme => meme.TopText)
            .Include(meme => meme.BottomText)
            .Include(meme => meme.Topics)
            .Where(meme => meme.MemeVisual.Id == visual.Id);

        memes = (toptext, bottomtext) switch
        {
            (null, null) => memes.Where(meme => meme.TopText == null && meme.BottomText == null),
            ({ }, null) => memes.Where(meme => meme.TopText().Id == toptext.Id && meme.BottomText == null),
            (null, { }) => memes.Where(meme => meme.BottomText().Id == bottomtext.Id && meme.TopText == null),
            ({ }, { }) => memes.Where(meme => meme.TopText().Id == toptext.Id && meme.BottomText().Id == bottomtext.Id)
        };
        
        return await memes.FirstOrDefaultAsync();
    }
}
