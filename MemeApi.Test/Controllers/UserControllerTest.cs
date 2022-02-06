using System.Threading.Tasks;
using FluentAssertions;
using MemeApi.Controllers;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using MemeApi.Test.utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MemeApi.Test.Controllers
{
    public class UserControllerTest
    {

        [Fact]
        public async Task GIVEN_DummyUser_WHEN_CreatingUser_THEN_UserIsCreatedWithProperValues()
        {
            await using var context = ContextUtils.CreateMemeTestContext();
            var controller = new UsersController(context, new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());

            // given
            var userDTO = new UserCreationDTO
            {
                Username = "Test",
                Email = "Test",
                Password = "Test"
            };

            // When
            var createTask = controller.CreateUser(userDTO);


            // Then
            var createdUser = await ActionResultUtils.ActionResultToValueAndAssertCreated(createTask);

            (await context.Users.CountAsync()).Should().Be(1);
            createdUser.Username.Should().Be(userDTO.Username);
            createdUser.Email.Should().Be(userDTO.Email);
            createdUser.PasswordHash.Should().NotBe(userDTO.Password);
        }

        [Fact]
        public async Task GIVEN_CreatedDummyUser_WHEN_GettingUser_THEN_UserHasProperValues()
        {
            await using var context = ContextUtils.CreateMemeTestContext();
            var controller = new UsersController(context, new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());

            // given
            var userDTO = new UserCreationDTO
            {
                Username = "Test",
                Email = "Test",
                Password = "Test"
            };

            var createdUser =
                await ActionResultUtils.ActionResultToValueAndAssertCreated(controller.CreateUser(userDTO));

            // When
            var getTask = controller.GetUser(createdUser.Id);

            // Then
            var foundUser = await ActionResultUtils.ActionResultToValueAndAssertOk(getTask);

            foundUser.Username.Should().Be(userDTO.Username);
            foundUser.Email.Should().Be(userDTO.Email);
            foundUser.PasswordHash.Should().NotBe(userDTO.Password);
        }

        [Fact]
        public async Task GIVEN_CreatedDummyUser_WHEN_Updating_THEN_UserIsUpdatedWithGivenValues()
        {
            await using var context = ContextUtils.CreateMemeTestContext();
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

            
            var createdUser = ActionResultUtils.ActionResultToValueAndAssertCreated(controller.CreateUser(userDTO));

            // When
            await controller.UpdateUser(createdUser.Id, updateDto);
            var getTask = controller.GetUser(createdUser.Id);

            // Then
            var foundUser = await ActionResultUtils.ActionResultToValueAndAssertOk(getTask);

            foundUser.Username.Should().Be(updateDto.NewUsername);
            foundUser.Email.Should().Be(updateDto.NewEmail);
            foundUser.PasswordHash.Should().NotBe(updateDto.NewPassword);
        }

        [Fact]
        public async Task GIVEN_CreatedDummyUser_WHEN_Deleting_THEN_UserIsDeleted()
        {
            await using var context = ContextUtils.CreateMemeTestContext();
            var controller = new UsersController(context, new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());

            // given
            var userDTO = new UserCreationDTO
            {
                Username = "Test",
                Email = "Test",
                Password = "Test"
            };
            
            var createdUser =
                await ActionResultUtils.ActionResultToValueAndAssertCreated(controller.CreateUser(userDTO));

            // When
            var deleteTask = controller.DeleteUser(createdUser.Id);

            // Then
            await ActionResultUtils.ActionResultAssertNoContent(deleteTask);
            (await context.Users.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task GIVEN_CreatedDummyUser_WHEN_LoggingIn_THEN_UserIsLoggedIn()
        {
            await using var context = ContextUtils.CreateMemeTestContext();
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

        [Fact]
        public async Task GIVEN_CreatedDummyUser_WHEN_LoggingInWithWrongPassword_THEN_Unauthorized()
        {
            await using var context = ContextUtils.CreateMemeTestContext();
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
                password = "WrongPassword"
            };

            await controller.CreateUser(userDTO);

            // When
            var result = controller.Login(loginDTO);

            // Then
            result.Should().NotBeNull();
            result.Should().BeOfType<UnauthorizedResult>();
        }
    }
}
