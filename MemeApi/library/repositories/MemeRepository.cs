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
    private readonly UserRepository _userRepository;
    private readonly MemeApiSettings _settings;
    public MemeRepository(MemeContext context, VisualRepository visualRepository, TextRepository textRepository, TopicRepository topicRepository, MemeApiSettings settings, UserRepository userRepository)
    {
        _context = context;
        _visualRepository = visualRepository;
        _textRepository = textRepository;
        _topicRepository = topicRepository;
        _settings = settings;
        _userRepository = userRepository;
    }

    public async Task<Meme?> CreateMeme(MemeCreationDTO memeDTO, string? userId = null)
    {
        var user = await _userRepository.GetUser(userId);
        var memeVisual = await _visualRepository.CreateMemeVisual(memeDTO.VisualFile, memeDTO.FileName, memeDTO.Topics, user);
        var meme = new Meme
        {
            Id = Guid.NewGuid().ToString(),
            VisualId = memeVisual.Id,
            Visual = memeVisual,
        };

        if (user != null) meme.Owner = user;

        //if (memeDTO.SoundFile != null)
        //{
        //    var memeSound = new MemeSound { Filename = memeDTO.SoundFile };
        //    _context.Sounds.Add(memeSound);
        //    meme.MemeSound = memeSound;
        //}

        if (memeDTO.TopText != null)
        {
            var toptext = await _textRepository.CreateText(memeDTO.TopText, MemeTextPosition.TopText, memeDTO.Topics, user);
            meme.TopText = toptext;
            meme.TopTextId = toptext.Id;
        }

        if (memeDTO.BottomText != null)
        {
            var bottomtext = await _textRepository.CreateText(memeDTO.BottomText, MemeTextPosition.BottomText, memeDTO.Topics, user);
            meme.BottomText = bottomtext;
            meme.BottomTextId = bottomtext.Id;
        }

        var topics = await _topicRepository.GetTopicsByNameForUser(memeDTO.Topics, userId );

        if (topics.Any(t => t == null)) return null;

        meme.Topics = topics.ToList();

        _context.Memes.Add(meme);
        await _context.SaveChangesAsync();
        return meme;
    }

    public async Task<bool> HasMemeCreatedInTimeSpan(DateTime start, DateTime end) 
    {
        return await _context.Memes.AnyAsync(m => start < m.CreatedAt && m.CreatedAt < end);
    }

    public async Task<Meme?> CreateMemeById(MemeCreationByIdDTO memeDTO, string? userId = null)
    {
        var memeVisual = await _visualRepository.GetVisual(memeDTO.VisualId);
        if (memeVisual == null) return null;

        var toptext = memeDTO.TopTextId != null ? await _textRepository.GetText(memeDTO.TopTextId) : null;
        var bottomtext = memeDTO.BottomTextId != null ? await _textRepository.GetText(memeDTO.BottomTextId) : null;
        var topics = await _topicRepository.GetTopicsByNameForUser(memeDTO.Topics, userId);

        if (topics.Any(t => t == null)) return null;

        return await UpsertByComponents(memeVisual,toptext, bottomtext);
    }

    public async Task<Meme> UpsertByComponents(MemeVisual visual, MemeText? topText, MemeText? bottomText, Topic? topic = null)
    {
        var meme = await FindByComponents(visual, topText, bottomText);
        topic ??= await _topicRepository.GetDefaultTopic();
        if (meme == null)
        {
            meme = new Meme
            {
                Id = Guid.NewGuid().ToString(),
                Visual = visual,
                TopText = topText,
                BottomText = bottomText,
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
        var list = await _context.Memes.Include(x => x.BottomText).ToListAsync();
        return await IncludeParts()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    private IIncludableQueryable<Meme, MemeText?> IncludeParts()
    {
        return _context.Memes
            .Include(m => m.Visual)
            .Include(m => m.Topics)
            .Include(m => m.TopText)
            .Include(m => m.BottomText);
    }
    public async Task<Meme> RandomMemeByComponents(string? visualId = null, string? topText = null, string? bottomText = null, string? topicName = null)
    {
        MemeVisual? visual = null;
        var defaultTopic = await _topicRepository.GetTopicByName(_settings.GetDefaultTopicName());

        if (defaultTopic == null) throw new ArgumentException("Default topic cannot be found");

        if (visualId != null) {
            visual = await _visualRepository.GetVisual(visualId);
        }

        if (visual == null)
        {
            visual = _visualRepository.GetRandomVisualInTopic(defaultTopic);
        }
        
        var topTextComponent = topText == null ? 
            _textRepository.GetRandomTextByTypeInTopic(MemeTextPosition.TopText, defaultTopic) 
            : await _textRepository.GetTextByContent(topText, MemeTextPosition.TopText);

        var bottomTextComponent = bottomText == null ?
            _textRepository.GetRandomTextByTypeInTopic(MemeTextPosition.BottomText, defaultTopic)
            : await _textRepository.GetTextByContent(bottomText, MemeTextPosition.BottomText);
        var topic = await _topicRepository.GetTopicByName(topicName);
        if (topic == null) topic = defaultTopic;

        var meme = await UpsertByComponents(visual, topTextComponent, bottomTextComponent, topic);
        return meme;
    }

    public bool HasMemeOfTheDay(DateTime date)
    {
        var memes = _context.Memes
            .Include(m => m.Topics)
            .Where(m =>
                m.Topics.Any(t => t.Name == _settings.GetMemeOfTheDayTopicName())
                ).ToList();
        return memes.Any(m => m.CreatedAt.Date == date.Date);
    }
    public async Task<Meme?> FindByComponents(MemeVisual visual, MemeText? topText = null, MemeText? bottomText = null)
    {
        var memes = _context.Memes
            .Include(meme => meme.Visual)
            .Include(meme => meme.TopText)
            .Include(meme => meme.BottomText)
            .Include(meme => meme.Topics)
            .Where(meme => meme.Visual.Id == visual.Id);

        memes = (topText, bottomText) switch
        {
            (null, null) => memes.Where(meme => meme.TopText == null && meme.BottomText == null),
            ({ }, null) => memes.Where(meme => meme.TopTextId == topText.Id && meme.BottomText == null),
            (null, { }) => memes.Where(meme => meme.BottomTextId == bottomText.Id && meme.TopText == null),
            ({ }, { }) => memes.Where(meme => meme.BottomTextId == topText.Id && meme.BottomTextId == bottomText.Id)
        };
        
        return await memes.FirstOrDefaultAsync();
    }
}
