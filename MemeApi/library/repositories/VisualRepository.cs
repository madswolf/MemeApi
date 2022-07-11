using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MemeApi.library.repositories
{
    public class VisualRepository
    {
        private readonly MemeContext _context;
        private readonly IFileSaver _fileSaver;
        private readonly IFileRemover _fileRemover;
        private static readonly Random Random = new();
        public VisualRepository(MemeContext context, IFileSaver fileSaver, IFileRemover fileRemover)
        {
            _context = context;
            _fileSaver = fileSaver;
            _fileRemover = fileRemover;
        }

        public async Task<List<MemeVisual>> GetVisuals()
        {
            return await _context.Visuals.ToListAsync();
        }

        public async Task<MemeVisual?> GetVisual(long id)
        {
            return await _context.Visuals.FindAsync(id);
        }


        private bool MemeVisualExists(long id)
        {
            return _context.Visuals.Any(e => e.Id == id);
        }


        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public async Task<MemeVisual> CreateMemeVisual(IFormFile visual)
        {
            var memeVisual = new MemeVisual()
            {
                Filename = visual.FileName
            };

            if (_context.Visuals.Any(x => x.Filename == visual.FileName))
            {
                memeVisual.Filename = RandomString(5) + memeVisual.Filename;
            }

            _fileSaver.SaveFile(visual, "uploads/");

            _context.Visuals.Add(memeVisual);
            await _context.SaveChangesAsync();
            return memeVisual;
        }

        public async Task<bool> RemoveMemeVisual(long id)
        {
            var memeVisual = await _context.Visuals.FindAsync(id);
            if (memeVisual == null)
            {
                return false;
            }

            _fileRemover.RemoveFile(Path.Combine("uploads/", memeVisual.Filename));

            _context.Visuals.Remove(memeVisual);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
