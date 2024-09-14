using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MemeApi.Controllers;
using MemeApi.library.repositories;
using MemeApi.library.Services;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using MemeApi.Test.library;
using MemeApi.Test.utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace MemeApi.Test.Controllers
{
    public class UserControllerTest : MemeTestBase
    {
        public UserControllerTest(IntegrationTestFactory databaseFixture) : base(databaseFixture)
        {
        }

        [Fact]
        public async Task GIVEN_CreatedDummyUser_WHEN_GettingUser_THEN_UserHasProperValues()
        {
            var controller = new UsersController(_signInManager, _userManager, _userRepository, new Mock<IMailSender>().Object, _settings);
            controller.ControllerContext.HttpContext = GetMockedHttpContext();

            // given
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "Test",
                Email = "Test",
                PasswordHash = "Test"
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            // When
            var getTask = await controller.GetUser(user.Id);

            // Then
            var foundUser = ActionResultUtils.ActionResultToValueAndAssertOk(getTask);

            foundUser?.UserName.Should().Be(user.UserName);
        }

        [Fact]
        public async Task GIVEN_CreatedDummyUser_WHEN_Updating_THEN_UserIsUpdatedWithGivenValues()
        {
            var controller = new UsersController(_signInManager, _userManager, _userRepository, new Mock<IMailSender>().Object, _settings);
            controller.ControllerContext.HttpContext = GetMockedHttpContext();

            // given
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "Test",
                Email = "Test",
                PasswordHash = "Test"
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            var updateDto = new UserUpdateDTO
            {
                NewUsername = "Test2",
                NewEmail = "Test2",
                NewPassword = "Test2",
            };
            SetUserNameIdentifier(controller, user.Id);

            // When
            await controller.UpdateUser(updateDto);

            // Then
            var foundUser = await _context.Users.FindAsync(user.Id);
            foundUser?.UserName.Should().Be(updateDto.NewUsername);
            foundUser?.Email.Should().Be(updateDto.NewEmail);
            foundUser?.Should().NotBe(updateDto.NewPassword);
        }

        //[Fact]
        //public async Task GIVEN_CreatedDummyUser_WHEN_Deleting_THEN_UserIsDeleted()
        //{
        //    var controller = new UsersController(_signInManager, _userManager, _userRepository, new Mock<IMailSender>().Object, //_settings);
        //
        //    // given
        //    var user = new User
        //    {
        //        Id = Guid.NewGuid().ToString(),
        //        UserName = "Test",
        //        Email = "Test",
        //        PasswordHash = "Test",
        //    };
        //    _context.Users.Add(user);
        //    _context.SaveChanges();
        //    SetUserNameIdentifier(controller, user.Id);
        //
        //    // When
        //    var deleteTask = await controller.DeleteUser();
        //
        //    // Then
        //    deleteTask.Should().BeOfType<OkResult>();
        //     _context.Users.Where(u => u.Id == user.Id).ToList().Should().BeEmpty();
        //}

        //[Fact]
        //public async Task GIVEN_CreatedDummyUser_WHEN_LoggingIn_THEN_UserIsLoggedIn()
        //{
        //    var controller = new UsersController(_signInManager, _userManager, _userRepository, new Mock<IMailSender>().Object, //_settings);
        //    controller.ControllerContext.HttpContext = GetMockedHttpContext();
        //
        //    var userDTO = new UserCreationDTO
        //    {
        //        Username = "Test",
        //        Email = "Test",
        //        Password = "Test"
        //    };
        //    await controller.Register(userDTO);
        //
        //    var loginDTO = new UserLoginDTO
        //    {
        //        Username = "Test",
        //        Password = "Test"
        //    };
        //
        //    // When
        //    var result = controller.Login(loginDTO);
        //
        //    // Then
        //    result.Should().NotBeNull();
        //    result.Should().BeOfType<OkObjectResult>();
        //}

        //[Fact]
        //public async Task GIVEN_CreatedDummyUser_WHEN_LoggingInWithWrongPassword_THEN_Unauthorized()
        //{
        //    var controller = new UsersController(_signInManager, _userManager, _userRepository, new Mock<IMailSender>().Object, //_settings);
        //    controller.ControllerContext.HttpContext = GetMockedHttpContext();
        //
        //    // given
        //    var user = new User
        //    {
        //        Id = Guid.NewGuid().ToString(),
        //        UserName = "Test",
        //        Email = "Test",
        //        PasswordHash = "Test",
        //    };
        //    _context.Users.Add(user);
        //
        //    var loginDTO = new UserLoginDTO
        //    {
        //        Username = "Test",
        //        Password = "WrongPassword"
        //    };
        //
        //    // When
        //    var result = controller.Login(loginDTO);
        //
        //    // Then
        //    result.Should().NotBeNull();
        //    result.Should().BeOfType<UnauthorizedResult>();
        //}
    }
}
