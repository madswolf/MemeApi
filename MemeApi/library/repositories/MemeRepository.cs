using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemeApi.Models.Context;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace MemeApi.library.repositories
{
    public class MemeRepository
    {
        private readonly MemeContext _context;
        public MemeRepository(MemeContext context)
        {
            _context = context;
        }

        public async Task<Meme> CreateMeme(MemeCreationDTO memeDTO)
        {
            var memeVisual = new MemeVisual { Filename = memeDTO.VisualFile };

            _context.Visuals.Add(memeVisual);

            var meme = new Meme
            {
                MemeVisual = memeVisual,
            };

            if (memeDTO.SoundFile != null)
            {
                var memeSound = new MemeSound { Filename = memeDTO.SoundFile };
                _context.Sounds.Add(memeSound);
                meme.MemeSound = memeSound;
            }

            if (memeDTO.Texts != null)
            {
                var memeTexts = memeDTO.Texts.Select(item =>
                {
                    var (text, position) = item;
                    var memeText = new MemeText
                    {
                        Text = text,
                        Position = (MemeTextPosition)Enum.Parse(typeof(MemeTextPosition), position)
                    };
                    _context.Texts.Add(memeText);
                    return memeText;
                });
                meme.MemeTexts = memeTexts.ToList();
            }

            _context.Memes.Add(meme);
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

        public Meme GetMeme(int id)
        {
            return IncludeParts()
                .First(m => m.Id == id);
        }

        private IIncludableQueryable<Meme, List<MemeText>> IncludeParts()
        {
            return _context.Memes
                .Include(m => m.MemeVisual)
                .Include(m => m.MemeSound)
                .Include(m => m.MemeTexts);
        }

        private bool MemeExists(int id)
        {
            return _context.Memes.Any(e => e.Id == id);
        }
    }
}
