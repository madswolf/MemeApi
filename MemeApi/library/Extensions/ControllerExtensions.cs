using System;
using System.Collections.Generic;
using System.Linq;
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


        public static int SumVotes(this Votable votable)
        {
            return votable.Votes.Aggregate(0, (acc, item) => acc + (item.Upvote ? 1 : -1));
        }

    }
}
