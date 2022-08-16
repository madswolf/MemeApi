using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemeApi.library.repositories
{
    public class TextRepository
    {
        private readonly MemeContext _context;

        public TextRepository(MemeContext context)
        {
            _context = context;
        }

        public async Task<List<MemeText>> GetTexts(MemeTextPosition? type = null)
        {
            if (type != null)
            {
                return await _context.Texts.Where(x => x.Position == type).ToListAsync();
            }
            
            return await _context.Texts.ToListAsync();
        }

        public async Task<MemeText> GetText(int id)
        {
            return await _context.Texts.FindAsync(id);
        }

        public async Task<bool> UpdateText(int id, string newMemeBottomText, MemeTextPosition? newMemeTextPosition = null)
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

        public async Task<MemeText> CreateText(string text, MemeTextPosition position)
        {
            var memeText = new MemeText
            {
                Text = text,
                Position = position
            };

            _context.Texts.Add(memeText);
            await _context.SaveChangesAsync();
            return memeText;
        }

        public async Task<bool> RemoveText(int id)
        {
            var memeBottomText = await _context.Texts.FindAsync(id);
            if (memeBottomText == null) return false;
            

            _context.Texts.Remove(memeBottomText);
            await _context.SaveChangesAsync();

            return true;
        }

        private bool MemeBottomTextExists(int id)
        {
            return _context.Texts.Any(e => e.Id == id);
        }
    }
}
