using System;
using System.Collections.Generic;
using System.Linq;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;

namespace MemeApi.library.Extensions;

public static class Extensions
{
    public static T RandomItem<T>(this List<T> list, string seed = "")
    {
        if(seed == "")
        {
            seed = Guid.NewGuid().ToString();
        }
        var random = new Random(seed.GetHashCode());
        
        return list[random.Next(list.Count)];
    }

    public static TopicDTO ToTopicDTO(this Topic t)
    {
        return new TopicDTO(t.Id, t.Name, t.Description, t.Owner.UserName, t.Moderators.Select(u => u.UserName).ToList(), t.CreatedAt, t.LastUpdatedAt);
    }

    public static TextDTO ToTextDTO(this MemeText text)
    {
        return new TextDTO(text.Id, text.Text, text.Position, text.Topics.Select(t => t.Name).ToList(), text.CreatedAt);
    }

    public static VisualDTO ToVisualDTO(this MemeVisual visual)
    {
        return new VisualDTO(visual.Id, visual.Filename, visual.Topics.Select(t => t.Name).ToList(), visual.CreatedAt);
    }

    public static UserInfoDTO ToUserInfo(this User u, string mediaHost)
    {
        return new UserInfoDTO(u.UserName, mediaHost + "profilepic/" + u.ProfilePicFile, u.Topics.Select(t => t.Name).ToList());
    }

    public static RandomComponentDTO ToRandomComponentDTO(this MemeText memeText)
    {
        return new RandomComponentDTO(memeText.Text, memeText.Id, memeText.SumVotes());
    }

    public static RandomComponentDTO ToRandomComponentDTO(this MemeVisual visual, string mediaHost)
    {
        return new RandomComponentDTO(mediaHost + "visual/" + visual.Filename, visual.Id, visual.SumVotes());
    }

    public static MemeDTO ToMemeDTO(this Meme meme)
    {
        var memeDTO = new MemeDTO(
            meme.Id,
            meme.MemeVisual.Filename,
            meme.Toptext != null ? meme.Toptext.Text : "",
            meme.BottomText != null ? meme.BottomText.Text : "",
            meme.Topics.Select(t => t.Name).ToList(),
            meme.CreatedAt
        );

        return memeDTO;
    }

    public static int SumVotes(this Votable votable)
    {
        var votes = votable.Votes ?? new List<Vote>();
        return votes.Aggregate(0, (acc, item) => acc + (item.Upvote ? 1 : -1));
    }
}
