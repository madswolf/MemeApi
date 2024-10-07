using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MemeApi.Controllers;
using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.library.Services;
using MemeApi.Models.DTO;
using MemeApi.Models.DTO.Dubloons;
using MemeApi.Models.Entity;
using MemeApi.Models.Entity.Dubloons;
using MemeApi.Models.Entity.Memes;
using MemeApi.Test.library;
using MemeApi.Test.utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        public async Task GIVEN_TwoUsersWithDubloons_WHEN_TransferingDubloons_THEN_SucessWithDubloonsMoved()
        {
            var controller = new UsersController(_signInManager, _userManager, _userRepository, new Mock<IMailSender>().Object, _settings);
            controller.ControllerContext.HttpContext = GetMockedHttpContext();

            // given
            var userId = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();
            var user = new User
            {
                Id = userId.ExternalUserIdToGuid(),
                UserName = "Test",
            };

            var user2 = new User
            {
                Id = userId2.ExternalUserIdToGuid(),
                UserName = "Test2",
            };
            var meme = new Meme
            {
                Id = Guid.NewGuid().ToString(),
                Visual = new MemeVisual { Id = Guid.NewGuid().ToString(), Filename = "test" }
            };
            var vote = new Vote
            {
                Id = Guid.NewGuid().ToString(),
                User = user,
                Element = meme,
            };

            var dailyVote = new DailyVote() {
                Id = Guid.NewGuid().ToString(),
                Dubloons = 100,
                Vote = vote,
                Owner = user
            };

            vote.DubloonEvent = dailyVote;

            _context.Users.Add(user);
            _context.Users.Add(user2);
            _context.Votes.Add(vote);
            _context.DubloonEvents.Add(dailyVote);
            _context.SaveChanges();

            // When
            SetUserNameIdentifier(controller, userId);
            var result = await controller.TransferDubloons(new DubloonTransferDTO
            {
                OtherUserId = userId2,
                DubloonsToTransfer = 100,
            });

            // Then
            result.Should().BeOfType<OkResult>();
            var resultUser = _context.Users.Include(u => u.DubloonEvents).First(u => u.Id == user.Id);
            var resultUser2 = _context.Users.Include(u => u.DubloonEvents).First(u => u.Id == user2.Id);
            resultUser.DubloonEvents.CountDubloons().Should().Be(0);
            resultUser2.DubloonEvents.CountDubloons().Should().Be(100);
        }

        [Fact]
        public async Task GIVEN_TwoUsersWithNotEnoughDubloons_WHEN_TransferingDubloons_THEN_Failure()
        {
            var controller = new UsersController(_signInManager, _userManager, _userRepository, new Mock<IMailSender>().Object, _settings);
            controller.ControllerContext.HttpContext = GetMockedHttpContext();

            // given
            var userId = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();
            var user = new User
            {
                Id = userId.ExternalUserIdToGuid(),
                UserName = "Test",
            };

            var user2 = new User
            {
                Id = userId2.ExternalUserIdToGuid(),
                UserName = "Test2",
            };
            var meme = new Meme
            {
                Id = Guid.NewGuid().ToString(),
                Visual = new MemeVisual { Id = Guid.NewGuid().ToString(), Filename = "test" }
            };
            var vote = new Vote
            {
                Id = Guid.NewGuid().ToString(),
                User = user,
                Element = meme,
            };

            var dailyVote = new DailyVote()
            {
                Id = Guid.NewGuid().ToString(),
                Dubloons = 10,
                Vote = vote,
                Owner = user
            };

            vote.DubloonEvent = dailyVote;

            _context.Users.Add(user);
            _context.Users.Add(user2);
            _context.Votes.Add(vote);
            _context.DubloonEvents.Add(dailyVote);
            _context.SaveChanges();

            // When
            SetUserNameIdentifier(controller, userId);
            var result = await controller.TransferDubloons(new DubloonTransferDTO
            {
                OtherUserId = userId2,
                DubloonsToTransfer = 100,
            });

            // Then
            result.Should().BeOfType<BadRequestObjectResult>();
            var resultUser = _context.Users.Include(u => u.DubloonEvents).First(u => u.Id == user.Id);
            var resultUser2 = _context.Users.Include(u => u.DubloonEvents).First(u => u.Id == user2.Id);
            resultUser.DubloonEvents.CountDubloons().Should().Be(10);
            resultUser2.DubloonEvents.CountDubloons().Should().Be(0);
        }

        [Fact]
        public async Task GIVEN_OneUser_WHEN_TransferingDubloonsToNonExistantUser_THEN_Failure()
        {
            var controller = new UsersController(_signInManager, _userManager, _userRepository, new Mock<IMailSender>().Object, _settings);
            controller.ControllerContext.HttpContext = GetMockedHttpContext();

            // given
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "Test",
            };


            _context.Users.Add(user);
            _context.SaveChanges();

            // When
            SetUserNameIdentifier(controller, user.Id);
            var result = await controller.TransferDubloons(new DubloonTransferDTO
            {
                OtherUserId = "test",
                DubloonsToTransfer = 100,
            });

            // Then
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GIVEN_OneUser_WHEN_TransferingDubloonsToThemself_THEN_Failure()
        {
            var controller = new UsersController(_signInManager, _userManager, _userRepository, new Mock<IMailSender>().Object, _settings);
            controller.ControllerContext.HttpContext = GetMockedHttpContext();

            // given
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "Test",
            };


            _context.Users.Add(user);
            _context.SaveChanges();

            // When
            SetUserNameIdentifier(controller, user.Id);
            var result = await controller.TransferDubloons(new DubloonTransferDTO
            {
                OtherUserId = user.Id,
                DubloonsToTransfer = 100,
            });

            // Then
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GIVEN_CreatedDummyUser_WHEN_Updating_THEN_UserIsUpdatedWithGivenValues()
        {
            var controller = new UsersController(_signInManager, _userManager, _userRepository, new Mock<IMailSender>().Object, _settings);
            controller.ControllerContext.HttpContext = GetMockedHttpContext();

            // given
            var userId = Guid.NewGuid().ToString();
            var user = new User
            {
                Id = userId.ExternalUserIdToGuid(),
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
            SetUserNameIdentifier(controller, userId);

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
