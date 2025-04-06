using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemeApi.library.Extensions;
using MemeApi.library.Services.Files;
using MemeApi.Models.Context;
using MemeApi.Models.DTO.Dubloons;
using MemeApi.Models.DTO.Memes;
using MemeApi.Models.Entity;
using MemeApi.Models.Entity.Dubloons;
using MemeApi.Models.Entity.Memes;
using Microsoft.EntityFrameworkCore;

namespace MemeApi.library.repositories;

public class VotableRepository
{
    private readonly MemeContext _context;
    private readonly MemeApiSettings _settings;
    private readonly IFileLoader _fileLoader;
    

    public VotableRepository(MemeContext context, MemeApiSettings settings, IFileLoader fileLoader)
    {
        _context = context;
        _settings = settings;
        _fileLoader = fileLoader;
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
        return await _context.Votables
            .Include(v => v.Topics)
            .ThenInclude(t => t.Moderators)
            .Include(v => v.Topics)
            .ThenInclude(t => t.Owner)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<VoteDTO> ChangeVote(Vote vote, Upvote upvote, int voteNumber)
    {
        vote.Upvote = upvote == Upvote.Upvote;
        vote.VoteNumber = voteNumber;
        vote.LastUpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return vote.ToVoteDTO();
    }

    public async Task<bool> DeleteVotable(Votable votable, User user, bool? hardDelete = null)
    {

        var topicUserModerates = votable.Topics
            .Where(t => t.Owner == user || t.Moderators.Exists(m => m == user))
            .FirstOrDefault();
        if (topicUserModerates == null)
        {
            return false;
        }
        votable.Topics.Remove(topicUserModerates);
        
        if(votable.Topics.Count == 0 && hardDelete != null && hardDelete == true)
        {
            var votes = _context.Votes.Where(vote => vote.ElementId == votable.Id);
            if (votes.Any())
            {
                return false; // you cannot currently delete votables with votes on them
            }
            _context.Votables.Remove(votable);
        }
        await _context.SaveChangesAsync();

        return true;
    }

    public IEnumerable<VotableComponentDTO> TopVotableInRange(
        string votableType,
        DateTime startDate,
        DateTime endDate,
        Topic topic,
        int takeCount = 5,
        bool orderAscending = true)
    {
        IQueryable<Votable> queryable =
            votableType.ToLower() switch
            {                
                "toptext" => _context.Texts.IncludeTopicsAndVotes().Where(t => t.Position == MemeTextPosition.TopText),
                "bottomtext" => _context.Texts.IncludeTopicsAndVotes().Where(t => t.Position == MemeTextPosition.BottomText),
                "visual" => _context.Visuals.IncludeTopicsAndVotes(),
                _ => _context.Memes.IncludeVotesAndVotesMemes(),
            };
        
        var itemsInRange =
            queryable.Where(v => startDate < v.CreatedAt && v.CreatedAt < endDate).ToList()
            .Where(v => v.Topics.Contains(topic))
            .Select(v => v.ToComponentDTO(_settings.GetMediaHost()));

        var orderedItems = orderAscending
            ? itemsInRange.OrderBy(v => v.voteAverage)
            : itemsInRange.OrderByDescending(v => v.voteAverage);

        return orderedItems.Take(takeCount);
    }

    public async Task<bool> RegenerateContentHash(string id, bool isMeme)
    {
        IQueryable<Votable> iQueryable;
        if (isMeme)
        {
            iQueryable = _context.Memes
                .Include(m => m.TopText)
                .Include(m => m.BottomText)
                .Include(m => m.Visual);
        }
        else
        {
            iQueryable = _context.Votables;
        }
            
        var votable = iQueryable.FirstOrDefault(v => v.Id == id);
        if (votable == null) return false;

        
        switch (votable)
        {
            case MemeVisual visual:
                var file = await _fileLoader.LoadFile("visual/" + visual.Filename);
                visual.ContentHash = file.ToContentHash();
                break;
            case MemeText text:
                text.ContentHash = (text.Text + text.Position).ToContentHash();
                break;
            case Meme meme:
                meme.ContentHash = meme.ToContentHash();
                break;
        }
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task ReassignReferences(string votableId, string targetId)
    {
        var votable = _context.Votables.Include(v => v.Topics).Include(v => v.Votes).First(v => v.Id == votableId);
        var target = _context.Votables.First(v => v.Id == targetId);
        await ReassignReferences(votable, target);
    }

    public async Task<Votable?> VerifyContentDuplicates(string votableId)
    {
        var targetVotable = _context.Votables
            .FirstOrDefault(v => v.Id == votableId);
        if (targetVotable == null) return null;

        var votables = _context.Votables
            .Include(v => v.Topics)
            .Include(v => v.Votes)
            .Where(v => v.ContentHash == targetVotable.ContentHash)
            .OrderBy(v => v.CreatedAt)
            .ToList();

        var votablesHaveTopicOverlap = votables.SelectMany(v => v.Topics).GroupBy(t => t.Id).Any(g => g.Count() != 1);
        if (votablesHaveTopicOverlap)
        {
            var (newListOfVotables, topics) =
                votables.Aggregate((new List<Votable>(), new List<string>()), (acc, item) =>
                {
                    var items = acc.Item1;
                    var topics = acc.Item2;
                    item.Topics = item.Topics.Where(t => !acc.Item2.Contains(t.Id)).ToList();
                    items.Add(item);
                    topics.AddRange(item.Topics.Select(t => t.Id));
                    return (items, topics);
                });

            var votablesWithNoTopicsAfterRemovingCollisions = newListOfVotables.Where(v => v.Topics.Count == 0).ToList();

            var oldestVotable = votables.First();
            foreach (var votable in votablesWithNoTopicsAfterRemovingCollisions)
            {
                await ReassignReferences(votable, oldestVotable);
            }
            
            _context.Votables.RemoveRange(votablesWithNoTopicsAfterRemovingCollisions);
            await _context.SaveChangesAsync();
            
            if (votablesWithNoTopicsAfterRemovingCollisions.Contains(targetVotable))
            {
                targetVotable.Id = "deleted";
            }
        }

        return targetVotable;
    }

    public async Task ReassignReferences(Votable votableWithReferencesToReassign, Votable target)
    {
        var topics = votableWithReferencesToReassign.Topics;
        var Votes = votableWithReferencesToReassign.Votes;
        
        if (votableWithReferencesToReassign is not Meme)
        {
            var id = votableWithReferencesToReassign.Id;
            var memesReferencingVotable = await _context.Memes
                .Where(m => m.BottomTextId == id || m.TopTextId == id || m.VisualId == id).ToListAsync();
            
            foreach (var meme in memesReferencingVotable)
            {
                if (meme.BottomTextId == id) meme.BottomTextId = target.Id;
                if (meme.TopTextId == id) meme.TopTextId = target.Id;
                if (meme.VisualId == id) meme.VisualId = target.Id;
            }
        }
        
        foreach (var vote in Votes)
        {
            vote.ElementId = target.Id;
        }
        
        if (topics.Count != 0)
        {
            throw new ArgumentException($"The votable with the Id:{votableWithReferencesToReassign.Id} is still referenced in the following topics: {topics.Select(t => t.Name)}");
        }

        await _context.SaveChangesAsync();
    }
}
