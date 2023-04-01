using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemeApi.Models;
using MemeApi.Models.Context;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;

namespace MemeApi.library.repositories
{
    public class MemeRepository
    {
        private readonly MemeContext _context;
        private readonly VisualRepository _visualRepository;
        private readonly TextRepository _textRepository;
        private readonly TopicRepository _topicRepository;
        private readonly IConfiguration _configuration;
        public MemeRepository(MemeContext context, VisualRepository visualRepository, TextRepository textRepository, IConfiguration configuration, TopicRepository topicRepository)
        {
            _context = context;
            _visualRepository = visualRepository;
            _textRepository = textRepository;
            _configuration = configuration;
            _topicRepository = topicRepository;
        }

        public async Task<Meme> CreateMeme(MemeCreationDTO memeDTO)
        {
            var memeVisual = await _visualRepository.CreateMemeVisual(memeDTO.VisualFile, memeDTO.FileName, memeDTO.Topics);

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

            if (memeDTO.Toptext != null)
            {
                meme.Toptext = await _textRepository.CreateText(memeDTO.Toptext, MemeTextPosition.TopText, memeDTO.Topics);
            }

            if (memeDTO.Bottomtext != null)
            {
                meme.BottomText = await _textRepository.CreateText(memeDTO.Bottomtext, MemeTextPosition.BottomText, memeDTO.Topics);
            }

            //TODO: fix multiple calls to get topics by reworking creation in createtext and createvisual to have a deeper version that does not save changes 
            var topics = await _topicRepository.GetTopicsByNameOrDefault(memeDTO.Topics);

            if (topics.Any(t => t == null)) return null;

            meme.Topics = topics.ToList();

            _context.Memes.Add(meme);
            await _context.SaveChangesAsync();
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

        public async Task<Meme> GetMeme(string id)
        {
            return await IncludeParts()
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        private IIncludableQueryable<Meme, MemeText> IncludeParts()
        {
            return _context.Memes
                .Include(m => m.MemeVisual)
                .Include(m => m.Topics)
                .Include(m => m.Toptext)
                .Include(m => m.BottomText);
        }

        public async Task<Meme?> FindByComponents(Votable visual, MemeText? toptext = null, MemeText? bottomtext = null)
        {
            var memes = _context.Memes
                .Include(meme => meme.MemeVisual)
                .Include(meme => meme.Toptext)
                .Include(meme => meme.BottomText)
                .Where(meme => meme.MemeVisual.Id == visual.Id);

            memes = (toptext, bottomtext) switch
            {
                (null, null) => memes.Where(meme => meme.Toptext == null && meme.BottomText == null),
                ({ }, null) => memes.Where(meme => meme.Toptext.Id == toptext.Id && meme.BottomText == null),
                (null, { }) => memes.Where(meme => meme.BottomText.Id == bottomtext.Id && meme.Toptext == null),
                ({ }, { }) => memes.Where(meme => meme.Toptext.Id == toptext.Id && meme.BottomText.Id == bottomtext.Id)
            };
            
            return await memes.FirstOrDefaultAsync();
        }
    }
}
