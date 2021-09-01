using FluentAssertions;
using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using MemeApi.Models.Context;
using MemeApi.Controllers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MemeApi.Test
{
    public class UserControllerTest
    {
        private readonly DbContextOptions<MemeContext> ContextOptions = new DbContextOptionsBuilder<MemeContext>().UseInMemoryDatabase("MemeList").Options;

        [Fact]
        public async Task Test1Async()
        {
            using (var context = new MemeContext(ContextOptions))
            {
                var controller = new UsersController(context, new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());

                // given

                var userDTO = new Models.UserCreationDTO
                {
                    Username = "Test",
                    Email = "Test",
                    password = "Test"
                };

                await controller.PostUser(userDTO);

                (await context.Users.CountAsync()).Should().Be(1);
                var user = await context.Users.FirstAsync();

                user.Username.Should().Be(userDTO.Username);
                user.Email.Should().Be(userDTO.Email);
                user.PasswordHash.Should().NotBe(userDTO.password);

                // Then

            }
            Assert.True(true);
        }
    }
}
