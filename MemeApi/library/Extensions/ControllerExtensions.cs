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
        public static T RandomItem<T>(this ControllerBase controller, List<T> list)
        {
            return list[Random.Shared.Next(list.Count)];
        }
    }
}
