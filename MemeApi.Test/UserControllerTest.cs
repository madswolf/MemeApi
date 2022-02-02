using FluentAssertions;
using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using MemeApi.Models.Context;
using MemeApi.Controllers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using MemeApi.Models;

namespace MemeApi.Test
{
    public class UserControllerTest
    {
        private readonly DbContextOptions<MemeContext> ContextOptions = new DbContextOptionsBuilder<MemeContext>().UseInMemoryDatabase("MemeList").Options;

        private MemeContext createTestContext()
        {
            var contextOptions = new DbContextOptionsBuilder<MemeContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            return new MemeContext(contextOptions);
        }

        [Fact]
        public async Task GIVEN_DummyUser_WHEN_CreatingUser_THEN_UserIsCreatedWithProperValues()
        {
            using (var context = createTestContext())
            {
                var controller = new UsersController(context, new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());

                // given

                var userDTO = new UserCreationDTO
                {
                    Username = "Test",
                    Email = "Test",
                    Password = "Test"
                };

                // When

                var createResult = (await controller.CreateUser(userDTO)).Result;


                // Then
                createResult.Should().NotBeNull();
                createResult.Should().BeOfType<CreatedAtActionResult>();
                var createdUser = ((CreatedAtActionResult)createResult).Value as User;  

                (await context.Users.CountAsync()).Should().Be(1);

                createdUser.Username.Should().Be(userDTO.Username);
                createdUser.Email.Should().Be(userDTO.Email);
                createdUser.PasswordHash.Should().NotBe(userDTO.Password);
            }
        }

        [Fact]
        public async Task GIVEN_CreatedDummyUser_WHEN_GettingUser_THEN_UserHasProperValues()
        {
            using (var context = createTestContext())
            {
                var controller = new UsersController(context, new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());

                // given

                var userDTO = new UserCreationDTO
                {
                    Username = "Test",
                    Email = "Test",
                    Password = "Test"
                };

                // When

                var createResult = (await controller.CreateUser(userDTO)).Result;
                var createdUser = ((CreatedAtActionResult)createResult).Value as User;  

                // Then
                var result = (await controller.GetUser(createdUser.Id)).Result;

                result.Should().NotBeNull();
                result.Should().BeOfType<OkObjectResult>();
                var foundUser = ((OkObjectResult)result).Value as User;   

                foundUser.Username.Should().Be(userDTO.Username);
                foundUser.Email.Should().Be(userDTO.Email);
                foundUser.PasswordHash.Should().NotBe(userDTO.Password);
            }
        }

        [Fact]
        public async Task GIVEN_CreatedDummyUser_WHEN_Updating_THEN_UserIsUpdatedWithGivenValues()
        {
            using (var context = createTestContext())
            {
                var controller = new UsersController(context, new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());

                // given

                var userDTO = new UserCreationDTO
                {
                    Username = "Test",
                    Email = "Test",
                    Password = "Test"
                };

                var updateDto = new UserUpdateDTO
                {
                    NewUsername = "Test2",
                    NewEmail = "Test2",
                    NewPassword = "Test2",
                };


                var createResult = (await controller.CreateUser(userDTO)).Result;
                var createdUser = ((CreatedAtActionResult)createResult).Value as User;

                // When
                await controller.UpdateUser(createdUser.Id, updateDto);

                // Then

                var result = (await controller.GetUser(createdUser.Id)).Result;
                result.Should().BeOfType<OkObjectResult>();
                var foundUser = ((OkObjectResult)result).Value as User;

                foundUser.Username.Should().Be(updateDto.NewUsername);
                foundUser.Email.Should().Be(updateDto.NewEmail);
                foundUser.PasswordHash.Should().NotBe(updateDto.NewPassword);
            }
        }

        [Fact]
        public async Task GIVEN_CreatedDummyUser_WHEN_Deleting_THEN_UserIsDeleted()
        {
            using (var context = createTestContext())
            {
                var controller = new UsersController(context, new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());

                // given

                var userDTO = new UserCreationDTO
                {
                    Username = "Test",
                    Email = "Test",
                    Password = "Test"
                };


                var createResult = (await controller.CreateUser(userDTO)).Result;
                var createdUser = ((CreatedAtActionResult)createResult).Value as User;

                // When
                var result = await controller.DeleteUser(createdUser.Id);

                // Then

                result.Should().NotBeNull();
                result.Should().BeOfType<NoContentResult>();
                (await context.Users.CountAsync()).Should().Be(0);
            }
        }

        [Fact]
        public async Task GIVEN_CreatedDummyUser_WHEN_LoggingIn_THEN_UserIsLoggedIn()
        {
            using (var context = createTestContext())
            {
                var controller = new UsersController(context, new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());

                // given

                var userDTO = new UserCreationDTO
                {
                    Username = "Test",
                    Email = "Test",
                    Password = "Test"
                };

                var loginDTO = new UserLoginDTO
                {
                    Username = "Test",
                    password = "Test"
                };
                await controller.CreateUser(userDTO);

                // When
                var result = controller.Login(loginDTO);

                // Then
                result.Should().NotBeNull();
                result.Should().BeOfType<OkObjectResult>();
            }
        }
    }
}
