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
    }
}
