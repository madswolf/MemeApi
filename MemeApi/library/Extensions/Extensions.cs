using System;
using System.Collections.Generic;
using System.Linq;
using MemeApi.Models;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;

namespace MemeApi.library.Extensions
{
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
            return new TopicDTO
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Owner = t.Owner.UserName,
                Moderators = t.Moderators.Select(u => u.UserName).ToList(),
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
            };
        }

        public static TextDTO ToTextDTO(this MemeText text)
        {
            return new TextDTO
            {
                Text = text.Text,
                Position = text.Position,
                Topics = text.Topics.Select(t => t.Name).ToList()
            };
        }

        public static VisualDTO ToVisualDTO(this MemeVisual visual)
        {
            return new VisualDTO
            {
                Filename = visual.Filename,
                Topics = visual.Topics.Select(t => t.Name).ToList()
            };
        }

        public static UserInfoDTO ToUserInfo(this User u, string mediaHost)
        {
            return new UserInfoDTO
            {
                UserName = u.UserName,
                ProfilePicURl = mediaHost + "profilepic/" + u.ProfilePicFile,
                Topics = u.Topics.Select(t => t.Name).ToList()
            };
        }

        public static RandomComponentDTO ToRandomComponentDTO(this MemeText memeText)
        {
            return new RandomComponentDTO
            {
                data = memeText.Text,
                id = memeText.Id,
                votes = memeText.SumVotes(),
            };
        }

        public static RandomComponentDTO ToRandomComponentDTO(this MemeVisual visual, string mediaHost)
        {
            return new RandomComponentDTO
            {
                data = mediaHost + "visual/" + visual.Filename,
                id = visual.Id,
                votes = visual.SumVotes()
            };
        }

        public static MemeDTO ToMemeDTO(this Meme meme)
        {
            var memeDTO = new MemeDTO
            {
                Id = meme.Id,
                MemeVisual = meme.MemeVisual.Filename,
                Topics = meme.Topics.Select(t => t.Name).ToList()
            };

            if( meme.Toptext != null ) memeDTO.Toptext = meme.Toptext.Text;
            if( meme.BottomText != null ) memeDTO.BottomText = meme.BottomText.Text;
            
            return memeDTO;
        }


        public static int SumVotes(this Votable votable)
        {
            var votes = votable.Votes ?? new List<Vote>();
            return votes.Aggregate(0, (acc, item) => acc + (item.Upvote ? 1 : -1));
        }
    }
}
