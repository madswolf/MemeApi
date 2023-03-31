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
            return await _context.Visuals.Include(x => x.Votes).Include(x => x.Topics).ToListAsync();
        }

        public async Task<MemeVisual?> GetVisual(int id)
        {
            return await _context.Visuals.Include(x => x.Votes).FirstOrDefaultAsync(v => v.Id == id);
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public async Task<MemeVisual> CreateMemeVisual(IFormFile visual, string filename, IEnumerable<string> topicNames = null)
        {
            var topics = await _topicRepository.GetTopicsByNameOrDefault(topicNames);
            var memeVisual = new MemeVisual()
            {
                Filename = filename,
                Topics = topics
            };

            if (_context.Visuals.Any(x => x.Filename == memeVisual.Filename))
            {
                memeVisual.Filename = RandomString(5) + memeVisual.Filename;
            }

            await _fileSaver.SaveFile(visual, "visual/", memeVisual.Filename);

            _context.Visuals.Add(memeVisual);
            await _context.SaveChangesAsync();
            return memeVisual;
        }

        public async Task<bool> RemoveMemeVisual(int id)
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
