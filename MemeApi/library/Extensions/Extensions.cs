using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MemeApi.library.Extensions;

public static class Extensions
{
    public static double CountDubloons(this IEnumerable<DubloonEvent> dubloonEvents)
    {
        return dubloonEvents.Select(d => d.Dubloons).Sum();
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

    public static T RandomItem<T>(this IEnumerable<T> source)
    {
        return source.PickRandom(1).Single();
    }

    public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
    {
        return source.Shuffle().Take(count);
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        return source.OrderBy(x => Guid.NewGuid());
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

    public static VoteDTO ToVoteDTO(this Vote vote) => new VoteDTO()
    {
        Id = vote.Id,
        VotableId = vote.ElementId,
        Upvote = vote.Upvote ? Upvote.Upvote : Upvote.Downvote,
        VoteNumber = vote.VoteNumber,
        Username = vote.User.UserName,
        CreatedAt = vote.CreatedAt,
        LastUpdateAt = vote.LastUpdatedAt
    };


    public static TopicDTO ToTopicDTO(this Topic t)
    {
        return new TopicDTO(t.Id, t.Name, t.Description, t.Owner.UserName(), t.Moderators.Select(u => u.UserName()).ToList(), t.CreatedAt, t.LastUpdatedAt);
    }

    public static TextDTO? ToTextDTO(this MemeText text)
    {
        if (text == null) return null;
        return new TextDTO(text.Id, text.Text, Enum.GetName(text.Position), text.Topics?.Select(t => t.Name).ToList(), text.CreatedAt);
    }

    public static VisualDTO ToVisualDTO(this MemeVisual visual)
    {
        return new VisualDTO(visual.Id, visual.Filename, visual.Topics.Select(t => t.Name).ToList(), visual.CreatedAt);
    }

    public static UserInfoDTO ToUserInfo(this User u, string mediaHost)
    {
        return new UserInfoDTO(u.Id, u.UserName(), mediaHost + "profilePic/" + u.ProfilePicFile, u.Topics?.Select(t => t.Name).ToList());
    }
    public static string UserName(this User u)
    {
        return u.UserName ?? "default username";
    }

    public static RandomComponentDTO ToRandomComponentDTO(this MemeText memeText)
    {
        return new RandomComponentDTO(memeText.Text, memeText.Id, memeText.SumVotes());
    }

    public static RandomComponentDTO ToRandomComponentDTO(this MemeVisual visual, string mediaHost)
    {
        return new RandomComponentDTO(mediaHost + "visual/" + visual.Filename, visual.Id, visual.SumVotes());
    }

    public static string ToFilenameString(this Meme meme)
    {
        return $"memeId_{meme.Id}_visualId_{meme.VisualId}_toptextId_{meme.TopTextId}_bottomtextId_{meme.BottomTextId}.png";
    }

    public static MemeDTO ToMemeDTO(this Meme meme)
    {
        var memeDTO = new MemeDTO(
            meme.Id,
            meme.Visual.Filename,
            meme.TopText?.ToTextDTO(),
            meme.BottomText?.ToTextDTO(),
            meme.Topics.Select(t => t.Name).ToList(),
            meme.CreatedAt
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

    public static int SumVotes(this Votable votable)
    {
        var votes = votable.Votes ?? [];
        return votes.Aggregate(0, (acc, item) => acc + (item.Upvote ? 1 : -1));
    }

    public static double CalculateDubloons(this Votable element, DateTime timestamp2)
    {
        var secondsDifference = (element.CreatedAt - timestamp2).Duration().TotalSeconds;

        const double initialDubloonCount = 100.0;
        const double dubloonCountAfterOneMInute = 10.0;
        const double firstMinute = 60.0; 
        const double threeDaysInSeconds = 3 * 24 * 60 * 60; 

        if (secondsDifference <= firstMinute)
            return initialDubloonCount - ((initialDubloonCount - dubloonCountAfterOneMInute) * (secondsDifference / firstMinute));
        else if (secondsDifference <= (firstMinute + threeDaysInSeconds))
            return dubloonCountAfterOneMInute - (dubloonCountAfterOneMInute * (secondsDifference - firstMinute / threeDaysInSeconds));
        else
            return 0;
    }
}
