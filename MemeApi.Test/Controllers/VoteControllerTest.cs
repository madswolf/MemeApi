using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using MemeApi.Controllers;
using MemeApi.Models;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using MemeApi.Test.utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MemeApi.Test.Controllers
{
    public class VoteControllerTest
    {
        [Fact]
        public async Task GIVEN_UserAndBottomText_WHEN_CreatingVote_THEN_VoteIsCreatedWithProperValues()
        {
            await using var context = ContextUtils.CreateMemeTestContext();
            var voteController = new VotesController(context);
            var textController = new MemeTextsController(context);
            var userController = new UsersController(context, new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());

            // given
            var memeText = "Test";
            var memePosition = MemeTextPosition.BottomText;

            var userDTO = new UserCreationDTO(){
                Username = "Test",
                Email = "Test",
                Password = "Test",
            };

            var createdMemeText =
                await ActionResultUtils.ActionResultToValueAndAssertCreated(
                    textController.CreateMemeBottomText(memeText, memePosition));

            var createdUser = await ActionResultUtils.ActionResultToValueAndAssertCreated(
                userController.CreateUser(userDTO));

            var voteDTO = new VoteDTO(){
                    UserID = createdUser.Id,
                    ElementID = createdMemeText.Id,
                    UpVote = true
            };

            // When
            var createVoteTask =  voteController.PostVote(voteDTO);

            // Then
            var createdVote = await ActionResultUtils.ActionResultToValueAndAssertCreated(createVoteTask);

            (await context.Texts.CountAsync()).Should().Be(1);
            createdVote.Upvote.Should().Be((bool)voteDTO.UpVote);
            createdVote.User.Should().Be(createdUser);
            createdVote.Element.Should().Be(createdMemeText);
        }

        [Fact]
        public async Task GIVEN_ExistingVote_WHEN_CreatingVote_THEN_VoteIsUpdated()
        {
            await using var context = ContextUtils.CreateMemeTestContext();
            var voteController = new VotesController(context);
            var textController = new MemeTextsController(context);
            var userController = new UsersController(context, new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());

            // given
            var memeText = "Test";
            var memePosition = MemeTextPosition.BottomText;

            var userDTO = new UserCreationDTO()
            {
                Username = "Test",
                Email = "Test",
                Password = "Test",
            };

            var createdMemeText =
                await ActionResultUtils.ActionResultToValueAndAssertCreated(
                    textController.CreateMemeBottomText(memeText, memePosition));

            var createdUser = await ActionResultUtils.ActionResultToValueAndAssertCreated(
                userController.CreateUser(userDTO));

            var voteDTO = new VoteDTO()
            {
                UserID = createdUser.Id,
                ElementID = createdMemeText.Id,
                UpVote = true
            };
            var createVoteTask = voteController.PostVote(voteDTO);
            var createdVote = await ActionResultUtils.ActionResultToValueAndAssertCreated(createVoteTask);
            var createdVoteUpVote = createdVote.Upvote;

            // When
            voteDTO.UpVote = false;
            var createVoteTask2 = voteController.PostVote(voteDTO);

            // Then
            var createdVote2 = await ActionResultUtils.ActionResultToValueAndAssertCreated(createVoteTask2);

            (await context.Texts.CountAsync()).Should().Be(1);
            createdVote2.User.Should().Be(createdUser);
            createdVote2.Element.Should().Be(createdMemeText);
            createdVote2.Upvote.Should().Be((bool)voteDTO.UpVote);
            createdVote2.Upvote.Should().NotBe(createdVoteUpVote);
            createdVote2.Id.Should().Be(createdVote.Id);
        }

        [Fact]
        public async Task GIVEN_ExistingVote_WHEN_CreatingVoteWithNullUpVote_THEN_VoteIsDeleted()
        {
            await using var context = ContextUtils.CreateMemeTestContext();
            var voteController = new VotesController(context);
            var textController = new MemeTextsController(context);
            var userController = new UsersController(context, new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());

            // given
            var memeText = "Test";
            var memePosition = MemeTextPosition.BottomText;

            var userDTO = new UserCreationDTO()
            {
                Username = "Test",
                Email = "Test",
                Password = "Test",
            };

            var createdMemeText =
                await ActionResultUtils.ActionResultToValueAndAssertCreated(
                    textController.CreateMemeBottomText(memeText, memePosition));

            var createdUser = await ActionResultUtils.ActionResultToValueAndAssertCreated(
                userController.CreateUser(userDTO));

            var voteDTO = new VoteDTO()
            {
                UserID = createdUser.Id,
                ElementID = createdMemeText.Id,
                UpVote = true
            };
            var createVoteTask = voteController.PostVote(voteDTO);
            var createdVote = await ActionResultUtils.ActionResultToValueAndAssertCreated(createVoteTask);

            // When
            voteDTO.UpVote = null;
            var createVoteTask2 = voteController.PostVote(voteDTO);

            // Then
            var result = (await createVoteTask2);
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<NoContentResult>();

            (await context.Votes.CountAsync()).Should().Be(0);
        }
    }
}
