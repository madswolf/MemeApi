using MemeApi.library.Extensions;
using MemeApi.Models.Context;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MemeApi.library.repositories;

public class VotableRepository
{
    private readonly MemeContext _context;
    private readonly MemeApiSettings _settings;

    public VotableRepository(MemeContext context, MemeApiSettings settings)
    {
        _context = context;
        _settings = settings;
    }

    public async Task<List<Vote>> GetVotes()
    {
        return await _context.Votes.ToListAsync();
    }

    public async Task<Vote?> GetVote(string id)
    {
        return await _context.Votes.FindAsync(id);
    }

    public async Task<List<Votable>> FindMany(IEnumerable<string> ids)
    {
        return await _context.Votables
            .Include(v => v.Topics)
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();
    }


    public Vote? FindByElementAndUser(Votable element, string userId)
    {
        IQueryable<Vote> queryable = _context.Votes
            .Include(x => x.Element)
            .Include(x => x.User);

        if(element.Topics.Any(x => x.Name == _settings.GetMemeOfTheDayTopicName()))
            queryable = queryable.Include(x => x.DubloonEvent);

        return queryable
            .Select(x => x)
            .SingleOrDefault(x => x.Element.Id == element.Id && x.User.Id == userId);
    }

    public async Task<Vote> CreateVote(Vote vote)
    {
        if (vote.Element.Topics.Any(x => x.Name == _settings.GetMemeOfTheDayTopicName()) && 
            DateTime.Now < vote.Element.CreatedAt.AddDays(3))
            vote.DubloonEvent = new DailyVote
            {
                Id = Guid.NewGuid().ToString(),
                Vote = vote,
                Owner = vote.User,
                Dubloons = vote.Element.CalculateDubloons(DateTime.UtcNow)
            };

        _context.Votes.Add(vote);
        await _context.SaveChangesAsync();
        return vote;
    }

    public async Task<bool> DeleteVote(string id)
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

    public async Task<Votable?> GetVotable(string id)
    {
        return await _context.Votables.FindAsync(id);
    }

    public async Task<VoteDTO> ChangeVote(Vote vote, Upvote upvote, int voteNumber)
    {
        vote.Upvote = upvote == Upvote.Upvote;
        vote.VoteNumber = voteNumber;
        vote.LastUpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return vote.ToVoteDTO();
    }

    public async Task<bool> DeleteVotable(Votable votable, User user)
    {

        var topicUserModerates = votable.Topics
            .Where(t => t.Owner == user || t.Moderators.Exists(m => m == user))
            .FirstOrDefault();
        if (topicUserModerates == null)
        {
            return false;
        }
        votable.Topics.Remove(topicUserModerates);
        if(votable.Topics.Count == 0)
        {
            _context.Votables.Remove(votable);
        }
        await _context.SaveChangesAsync();

        return true;
    }
}
