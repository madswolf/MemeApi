using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MemeApi.Models.DTO;
using MemeApi.Models.DTO.Dubloons;
using MemeApi.Models.DTO.Lotteries;
using MemeApi.Models.DTO.Memes;
using MemeApi.Models.Entity;
using MemeApi.Models.Entity.Dubloons;
using MemeApi.Models.Entity.Lottery;
using MemeApi.Models.Entity.Memes;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace MemeApi.library.Extensions;

public static class Extensions
{
    public static T ToVotableOfType<T>(this Votable votable) where T : Votable, new()
    {
        if (votable is not T votableOfType)
        {
            Console.WriteLine($"Votable with content hash: {votable.ContentHash} and Id: {votable.Id} is not of Type: {typeof(T)}");
            return new T();
        }
        
        return votableOfType;
    }
    public static string ToContentHash(this Meme meme)
    {
        return $"{meme.Visual.ContentHash}{meme.TopText?.ContentHash}{meme.BottomText?.ContentHash}".ToContentHash();
    }
    
    public static string PrependRandomString(this string baseString)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var prefix = new string(Enumerable.Repeat(chars, 5)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        
        return prefix + baseString;
    }
    
    public static string ToContentHash(this string? text)
    {
        return text == null ? "" : Encoding.UTF8.GetBytes(text).ToContentHash();
    }
    
    public static string ToContentHash(this byte[] content)
    {
        using var sha256 = SHA256.Create();
        return BitConverter.ToString(sha256.ComputeHash(content));
    }
    public static IIncludableQueryable<T, List<Topic>> IncludeTopicsAndVotes<T>(this DbSet<T> set) where T : Votable
    {
        return set.Include(v => v.Owner).Include(v => v.Votes).Include(v => v.Topics);
    }
    
    public static IIncludableQueryable<Meme, User> IncludeVotesAndVotesMemes(this DbSet<Meme> set)
    {
        return set.IncludeTopicsAndVotes()
            .Include(m => m.BottomText)
            .ThenInclude(t => t.Votes)
            .Include(m => m.BottomText)
            .ThenInclude(t => t.Owner)
            
            .Include(m => m.TopText)
            .ThenInclude(t => t.Votes)
            .Include(m => m.TopText)
            .ThenInclude(t => t.Owner)
            
            .Include(meme => meme.Visual)
            .ThenInclude(v => v.Votes)
            .Include(meme => meme.Visual)
            .ThenInclude(v => v.Owner);
    }
    public static DateTime TruncateToSeconds(this DateTime dateTime)
    {
        return dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerSecond));

    }
    public static async Task<byte[]> GetBytes(this IFormFile formFile)
    {
        await using var memoryStream = new MemoryStream();
        await formFile.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }
    public static double CountDubloons(this IEnumerable<DubloonEvent> dubloonEvents)
    {
        return dubloonEvents.Select(d => d.Dubloons).Sum();
    }

    public static string ExternalUserIdToGuid(this string externalUserId)
    {
        return new Guid(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(externalUserId))).ToString();
    }

    public static T RandomItem<T>(this List<T> list, string seed = "")
    {
        if (seed == "")
        {
            seed = Guid.NewGuid().ToString();
        }
        var random = new Random(seed.GetHashCode());

        return list[random.Next(list.Count)];
    }

    public static MemeText RandomItem(this IQueryable<MemeText> list, string seed = "")
    {
        if (seed == "")
        {
            seed = Guid.NewGuid().ToString();
        }
        var random = new Random(seed.GetHashCode());
        var skip = random.Next(list.Count());
        return list.OrderBy(x => x.Id).Skip(skip).First();
    }

    public static VoteDTO ToVoteDTO(this Vote vote) => new()
    {
        Id = vote.Id,
        VotableId = vote.ElementId,
        Upvote = vote.Upvote ? Upvote.Upvote : Upvote.Downvote,
        VoteNumber = vote.VoteNumber,
        Username = vote.User.UserName,
        CreatedAt = vote.CreatedAt,
        LastUpdateAt = vote.LastUpdatedAt
    };
    
    public static LotteryDTO ToLotteryDTO(this Lottery lottery, string mediaHost) => new()
    {
        Id = lottery.Id,
        Name = lottery.Name,
        TicketCost = lottery.TicketCost,
        Brackets = lottery.Brackets.Select(bracket => bracket.ToLotteryBracketDTO(mediaHost)).ToList(),
    };
    
    public static LotteryBracketDTO ToLotteryBracketDTO(this LotteryBracket bracket, string mediaHost) => new()
    {
        BracketId = bracket.Id,
        BracketName = bracket.Name,
        BracketProbabilityWeight = bracket.ProbabilityWeight,
        Items = bracket.Items.Select(item => item.ToLotteryItemDTO(mediaHost)).ToList(),
    };

    public static LotteryItemDTO ToLotteryItemDTO(this LotteryItem item, string mediaHost) => new()
    {
        ItemId = item.Id,
        ItemName = item.Name,
        InitialItemCount = item.ItemCount,
        CurrentItemCount = item.ItemCount - item.Tickets.Count,
        ItemThumbnail = mediaHost + "lotteryitems/"+ item.ThumbNailFileName
    };

    public static TopicDTO ToTopicDTO(this Topic t)
    {
        return new TopicDTO(t.Id, t.Name, t.Description, t.Owner.UserName(), t.Moderators.Select(u => u.UserName()).ToList(), t.CreatedAt, t.LastUpdatedAt);
    }

    public static TextDTO? ToTextDTO(this MemeText text, string mediaHost)
    {
        if (text == null) return null;
        return new TextDTO(text.Id, text.Text, Enum.GetName(text.Position), text.Owner.ToUserInfo(mediaHost), text.Topics?.Select(t => t.Name).ToList(), text.CreatedAt);
    }

    public static VisualDTO ToVisualDTO(this MemeVisual visual, string mediaHost)
    {
        return new VisualDTO(visual.Id, mediaHost + "visual/" + visual.Filename, visual.Owner.ToUserInfo(mediaHost), visual.Topics?.Select(t => t.Name).ToList(), visual.CreatedAt);
    }

    public static string ToThumbnailUrl(this LotteryItem item, string mediaHost)
    {
        return mediaHost + "lotteryitems/" + item.ThumbNailFileName;
    }

    public static UserInfoDTO? ToUserInfo(this User? u, string mediaHost)
    {
        if (u == null) return null;
        return new UserInfoDTO(u.Id, u.UserName(), mediaHost + "profilePic/" + u.ProfilePicFile, u.Topics?.Select(t => t.Name).ToList());
    }
    public static string UserName(this User u)
    {
        return u.UserName ?? "default username";
    }

    public static string ToFilenameString(this Meme meme)
    {
        return $"memeId_{meme.Id}_visualId_{meme.VisualId}_toptextId_{meme.TopTextId}_bottomtextId_{meme.BottomTextId}.png";
    }

    public static MemeDTO ToMemeDTO(this Meme meme, string mediaHost, byte[]? renderedMeme = null)
    {
        var memeDTO = new MemeDTO(
            meme.Id,
            meme.Visual.ToVisualDTO(mediaHost),
            meme.TopText?.ToTextDTO(mediaHost),
            meme.BottomText?.ToTextDTO(mediaHost),
            meme.Owner.ToUserInfo(mediaHost),
            meme.Topics.Select(t => t.Name).ToList(),
            meme.CreatedAt,
            renderedMeme
        );

        return memeDTO;
    }

    public static string ToTopText(this Meme meme)
    {
        return meme.TopText != null ? meme.TopText.Text : "";
    }

    public static string ToBottomText(this Meme meme)
    {
        return meme.BottomText != null ? meme.BottomText.Text : "";
    }

    public static bool CanUserPost(this Topic topic, string? userId = null)
    {
        var isRestricted = topic.HasRestrictedPosting;
        var isOwner = topic.Owner != null && topic.Owner?.Id == userId;
        var isModerator = topic.Moderators.Any(m => m != null &&  m.Id == userId);

        return (!isRestricted || isOwner || isModerator);
    }

    public static double CalculateDubloons(this Votable element, DateTime timestamp2)
    {
        var secondsDifference = (element.CreatedAt - timestamp2).Duration().TotalSeconds;

        double initialDubloonCount = 100.0;
        double lowerBoundAfterFirstDecayPhase = 75.0;
        double lowerBoundAfterSecondDecayPhase = 10.0;
        double beginningOfFirstDecayPhase = TimeSpan.FromHours(1).TotalSeconds;
        double endOfFirstDecayPhase = TimeSpan.FromHours(2).TotalSeconds; 
        double endOfSecondDecayPhase = TimeSpan.FromHours(3).TotalSeconds;
        double endOfThirdDecayPhase = TimeSpan.FromDays(3).TotalSeconds;

        if (secondsDifference <= beginningOfFirstDecayPhase)
            return initialDubloonCount;
        if (secondsDifference <= endOfFirstDecayPhase)
        {
            return Interpolate(
                secondsDifference,
                upperDubloonBound: initialDubloonCount,
                lowerDubloonBound: lowerBoundAfterFirstDecayPhase,
                lowerDecayBound: beginningOfFirstDecayPhase,
                upperDecayBound: endOfFirstDecayPhase);
        }

        if (secondsDifference <= (endOfSecondDecayPhase))
        {
            return Interpolate(
                secondsDifference,
                upperDubloonBound: lowerBoundAfterFirstDecayPhase,
                lowerDubloonBound: lowerBoundAfterSecondDecayPhase,
                lowerDecayBound: endOfFirstDecayPhase,
                upperDecayBound: endOfSecondDecayPhase);
        }

        if (secondsDifference <= (endOfThirdDecayPhase))
        {
            return Interpolate(
                secondsDifference,
                upperDubloonBound: lowerBoundAfterSecondDecayPhase,
                lowerDubloonBound: 0,
                lowerDecayBound: endOfSecondDecayPhase,
                upperDecayBound: endOfThirdDecayPhase);
        }

        return 0;
    }

    private static double Interpolate(double secondsDifference, double upperDubloonBound, double lowerDubloonBound, double lowerDecayBound, double upperDecayBound)
    {
        var porpotion = CalculatePortotionalDecay(secondsDifference, lowerDecayBound, upperDecayBound);

        return LinearInterpolation(upperDubloonBound, lowerDubloonBound, porpotion);
    }

    public static double LinearInterpolation(double start, double end, double porpotion)
    {
        return start + (end - start) * porpotion;
    }
    public static double CalculatePortotionalDecay(double secondsDifference, double lowerDecayBound, double maxDecayBound)
    {
        return ((secondsDifference - lowerDecayBound) / (maxDecayBound - lowerDecayBound));
    }
}
