using MemeApi.Models.Context;
using Microsoft.EntityFrameworkCore;
using System;

namespace MemeApi.Test.utils
{
    internal class ContextUtils
    {
        public static MemeContext CreateMemeTestContext()
        {
            var contextOptions = new DbContextOptionsBuilder<MemeContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            return new MemeContext(contextOptions);
        }
    }
}
