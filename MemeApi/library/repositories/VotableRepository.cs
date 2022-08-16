using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemeApi.Models;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemeApi.library.repositories
{
    public class VotableRepository
    {
        private readonly MemeContext _context;

        public VotableRepository(MemeContext context)
        {
            _context = context;
        }

        public async Task<List<Vote>> GetVotes()
        {
            return await _context.Votes.ToListAsync();
        }

        public async Task<Vote> GetVote(int id)
        {
            var vote = await _context.Votes.FindAsync(id);
            return vote;
        }

        public async Task<List<Votable>> FindMany(IEnumerable<int> ids)
        {
            return await _context.Votables
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();
        }


        public Vote? FindByElementAndUser(Votable element, int userId)
        {
            return _context.Votes
                .Select(x => x)
                .Include(x => x.Element)
                .Include(x => x.User)
                .SingleOrDefault(x => x.Element.Id == element.Id && x.User.Id == userId);
        }

        public async Task<Vote> CreateVote(Vote vote)
        {
            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();
            return vote;
        }

        public async Task<bool> DeleteVote(int id)
        {
            var vote = await GetVote(id);
            if (vote == null)
            {
                return false;
            }

            _context.Votes.Remove(vote);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
