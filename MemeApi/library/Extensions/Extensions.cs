﻿using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MemeApi.library.Extensions;

public static class Extensions
{
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
        return list.OrderBy(x => x.VotableId).Skip(skip).First();
    }

    public static TopicDTO ToTopicDTO(this Topic t)
    {
        return new TopicDTO(t.Id, t.Name, t.Description, t.Owner.UserName(), t.Moderators.Select(u => u.UserName()).ToList(), t.CreatedAt, t.LastUpdatedAt);
    }

    public static TextDTO? ToTextDTO(this MemeText text)
    {
        if (text == null) return null;
        return new TextDTO(text.VotableId, text.Text, text.Position, text.Votable.Topics?.Select(t => t.Name).ToList(), text.Votable.CreatedAt);
    }

    public static VisualDTO ToVisualDTO(this MemeVisual visual)
    {
        return new VisualDTO(visual.VotableId, visual.Filename, visual.Votable.Topics.Select(t => t.Name).ToList(), visual.Votable.CreatedAt);
    }

    public static UserInfoDTO ToUserInfo(this User u, string mediaHost)
    {
        return new UserInfoDTO(u.UserName(), mediaHost + "profilePic/" + u.ProfilePicFile, u.Topics.Select(t => t.Name).ToList());
    }
    public static string UserName(this User u)
    {
        return u.UserName ?? "default username";
    }
    public static MemeText TopText(this Meme u)
    {
        return u.TopText ?? new MemeText { VotableId = "", Position = MemeTextPosition.TopText };
    }

    public static MemeText BottomText(this Meme u)
    {
        return u.BottomText ?? new MemeText { VotableId = "", Position = MemeTextPosition.BottomText };

    }

    public static RandomComponentDTO ToRandomComponentDTO(this MemeText memeText)
    {
        return new RandomComponentDTO(memeText.Text, memeText.VotableId, memeText.Votable.SumVotes());
    }

    public static RandomComponentDTO ToRandomComponentDTO(this MemeVisual visual, string mediaHost)
    {
        return new RandomComponentDTO(mediaHost + "visual/" + visual.Filename, visual.VotableId, visual.Votable.SumVotes());
    }

    public static MemeDTO ToMemeDTO(this Meme meme)
    {
        var memeDTO = new MemeDTO(
            meme.VotableId,
            meme.Visual.Filename,
            meme.TopText?.ToTextDTO(),
            meme.BottomText?.ToTextDTO(),
            meme.Votable.Topics.Select(t => t.Name).ToList(),
            meme.Votable.CreatedAt
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
}
