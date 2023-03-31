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

            if (memeDTO.Topics == null)
            {
                memeDTO.Topics = new List<string> { _configuration["Topic.Default.Topicname"] };
            }

            var topics = await Task.WhenAll(
                memeDTO.Topics
                .Select(async topicName => await _topicRepository.GetTopicByName(topicName))
            );

            if (topics.Any(t => t == null)) return null;
            var topicList = topics.ToList();
            var memeVisual = await _visualRepository.CreateMemeVisual(memeDTO.VisualFile, memeDTO.FileName, topicList);

            var meme = new Meme
            {
                MemeVisual = memeVisual,
                Topics = topicList
            };

            //if (memeDTO.SoundFile != null)
            //{
            //    var memeSound = new MemeSound { Filename = memeDTO.SoundFile };
            //    _context.Sounds.Add(memeSound);
            //    meme.MemeSound = memeSound;
            //}

            if (memeDTO.Toptext != null)
            {
                meme.Toptext = await _textRepository.CreateText(memeDTO.Toptext, MemeTextPosition.TopText, topicList);
            }

            if (memeDTO.Bottomtext != null)
            {
                meme.BottomText = await _textRepository.CreateText(memeDTO.Bottomtext, MemeTextPosition.BottomText, topicList);
            }

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
        public async Task<bool> DeleteMeme(int id)
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

        internal Task GetRandomMemes()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ModifyMeme(int id, Meme meme)
        {
            _context.Entry(meme).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemeExists(id))
                {
                    return true;
                }

                throw;
            }

            return false;
        }

        public async Task<List<Meme>> GetMemes()
        {
            return await IncludeParts()
                .ToListAsync();
        }

        public async Task<Meme> GetMeme(int id)
        {
            return await IncludeParts()
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        private IIncludableQueryable<Meme, MemeText> IncludeParts()
        {
            return _context.Memes
                .Include(m => m.MemeVisual)
                .Include(m => m.Topics)
                .Include(m => m.MemeSound)
                .Include(m => m.Toptext)
                .Include(m => m.BottomText);
        }

        private bool MemeExists(int id)
        {
            return _context.Memes.Any(e => e.Id == id);
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
