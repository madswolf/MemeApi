using System;
using System.Collections.Generic;
using System.Linq;
using MemeApi.Models;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;

namespace MemeApi.library.Extensions
{
    public static class MyExtensions
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

        public static UserInfoDTO ToUserInfo(this User u, string mediaHost)
        {
            return new UserInfoDTO
            {
                UserName = u.UserName,
                ProfilePicURl = mediaHost + "profilepic/" + u.ProfilePicFile
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
            return new MemeDTO
            {
                MemeVisual = meme.MemeVisual,
                BottomText = meme.BottomText,
                Toptext = meme.Toptext,
                Topics = meme.Topics.Select(t => t.Name).ToList()
            };
        }


        public static int SumVotes(this Votable votable)
        {
            var votes = votable.Votes ?? new List<Vote>();
            return votes.Aggregate(0, (acc, item) => acc + (item.Upvote ? 1 : -1));
        }
    }
}
