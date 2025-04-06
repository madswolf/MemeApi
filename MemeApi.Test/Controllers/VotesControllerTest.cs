using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MemeApi.Controllers;
using MemeApi.library.Extensions;
using MemeApi.Models.DTO.Dubloons;
using MemeApi.Models.Entity;
using MemeApi.Models.Entity.Memes;
using MemeApi.Test.library;
using MemeApi.Test.utils;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MemeApi.Test.Controllers;

public class VotesControllerTest : MemeTestBase
{
    public VotesControllerTest(IntegrationTestFactory databaseFixture) : base(databaseFixture)
    {
    }


    [Theory]
    [InlineData(4, Upvote.Downvote)]
    [InlineData(5, Upvote.Upvote)]
    public async Task GIVEN_ExistingMemeOfTheDay_WHEN_VotingAsNewExternalUserWithBot_THEN_UserIsCreatedAndVoteStatusIsCorrect(int votenumber, Upvote expected)
    {
        var controller = new VotesController(_votableRepository, _textRepository, _visualRepository, _memeRepository, _userRepository, _settings);
        controller.ControllerContext.HttpContext = GetMockedHttpContext();
        controller.Request.Headers["Bot_Secret"] = _settings.GetBotSecret();
        var memeOfTheDayTopic = await _topicRepository.GetTopicByName(_settings.GetMemeOfTheDayTopicName());
        var meme = new Meme
        {
            Id = Guid.NewGuid().ToString(),
            Visual = new MemeVisual
            {
                Id = Guid.NewGuid().ToString(),
                Filename = "stuff",
                ContentHash = "",
            },
            ContentHash = "",
            Topics = [memeOfTheDayTopic]
        };

        _context.Memes.Add(meme);
        await _context.SaveChangesAsync();

        // given

        var postVoteDTO = new PostVoteDTO
        {
            ElementIDs = [meme.Id],
            VoteNumber = votenumber,
            ExternalUserID = Guid.NewGuid().ToString(),
            ExternalUserName = "Test",
        };

        // When
        var creationTask = await controller.PostVote(postVoteDTO);

        // Then
        var createdVote = ActionResultUtils.ActionResultToValueAndAssertCreated(creationTask);

        (await _context.Votes.CountAsync()).Should().Be(1);
        (await _context.Users.CountAsync()).Should().Be(2);
        (await _context.DubloonEvents.CountAsync()).Should().Be(1);

        var users = await _context.Users.ToListAsync();
        var externalUser = users.FirstOrDefault(x => x.Id == postVoteDTO.ExternalUserID.ExternalUserIdToGuid());

        createdVote.Should().NotBeNull();
        createdVote?.Upvote.Should().Be(expected);
        externalUser.Should().NotBeNull();
    }

    [Fact]
    public async Task GIVEN_ExistingMemeOfTheDay_WHEN_VotingAsExistingExternalUserWithBot_THEN_UserIsNotCreated()
    {
        var controller = new VotesController(_votableRepository, _textRepository, _visualRepository, _memeRepository, _userRepository, _settings);
        controller.ControllerContext.HttpContext = GetMockedHttpContext();
        controller.Request.Headers["Bot_Secret"] = _settings.GetBotSecret();
        var memeOfTheDayTopic = await _topicRepository.GetTopicByName(_settings.GetMemeOfTheDayTopicName());
        var meme = new Meme
        {
            Id = Guid.NewGuid().ToString(),
            Visual = new MemeVisual
            {
                Id = Guid.NewGuid().ToString(),
                ContentHash = "",
                Filename = "stuff"
            },
            ContentHash = "",
            Topics = [memeOfTheDayTopic]
        };
        var userId = Guid.NewGuid().ToString();

        var user = new User
        {
            // External users take use the has of a string to create their Guid
            // so we must create it already with that conversion
            Id = userId.ExternalUserIdToGuid(),
            UserName = "Test",
        };

        _context.Memes.Add(meme);
        await _context.SaveChangesAsync();

        // given

        var postVoteDTO = new PostVoteDTO
        {
            ElementIDs = [meme.Id],
            VoteNumber = 5,
            ExternalUserID = userId,
            ExternalUserName = user.UserName,
        };

        // When
        var creationTask = await controller.PostVote(postVoteDTO);

        // Then
        ActionResultUtils.ActionResultToValueAndAssertCreated(creationTask);

        (await _context.Votes.CountAsync()).Should().Be(1);
        (await _context.Users.CountAsync()).Should().Be(2);

        var users = await _context.Users.ToListAsync();
        var externalUser = users.FirstOrDefault(x => x.Id == postVoteDTO.ExternalUserID.ExternalUserIdToGuid());
        externalUser.Should().NotBeNull();
    }

    [Fact]
    public async Task GIVEN_ExistingMemeOfTheDayAndVote_WHEN_VotingWithBot_THEN_VoteChanged()
    {
        var controller = new VotesController(_votableRepository, _textRepository, _visualRepository, _memeRepository, _userRepository, _settings);
        controller.ControllerContext.HttpContext = GetMockedHttpContext();
        controller.Request.Headers["Bot_Secret"] = _settings.GetBotSecret();
        var memeOfTheDayTopic = await _topicRepository.GetTopicByName(_settings.GetMemeOfTheDayTopicName());
        var meme = new Meme
        {
            Id = Guid.NewGuid().ToString(),
            Visual = new MemeVisual
            {
                Id = Guid.NewGuid().ToString(),
                Filename = "stuff",
                ContentHash = ""
            },
            ContentHash = "",
            Topics = [memeOfTheDayTopic]
        };
        var userId = Guid.NewGuid().ToString();

        var user = new User
        {
            // External users take use the has of a string to create their Guid
            // so we must create it already with that conversion
            Id = userId.ExternalUserIdToGuid(),
            UserName = "Test",
        };

        var vote = new Vote
        {
            Id = new Guid().ToString(),
            ElementId = meme.Id,
            VoteNumber = 5,
            Upvote = true,
            User = user,
        };

        _context.Memes.Add(meme);
        _context.Votes.Add(vote);
        await _context.SaveChangesAsync();

        // given

        var postVoteDTO = new PostVoteDTO
        {
            ElementIDs = [meme.Id],
            VoteNumber = 4,
            ExternalUserID = userId,
            ExternalUserName = user.UserName,
        };

        // When
        var actionsResult = await controller.PostVote(postVoteDTO);

        // Then
        ActionResultUtils.ActionResultToValueAndAssertOk(actionsResult);

        (await _context.Votes.CountAsync()).Should().Be(1);

        var resultVote = await _context.Votes.FirstOrDefaultAsync(x => x.Id == vote.Id);
        resultVote.Should().NotBeNull();
        resultVote?.VoteNumber.Should().Be(postVoteDTO.VoteNumber);
        resultVote.Upvote.Should().Be(false);
    }

    [Fact]
    public async Task WHEN_VotingAsExistingExternalUserWithoutSecret_THEN_NotFound()
    {
        var controller = new VotesController(_votableRepository, _textRepository, _visualRepository, _memeRepository, _userRepository, _settings);
        controller.ControllerContext.HttpContext = GetMockedHttpContext();

        var meme = new Meme
        {
            Id = Guid.NewGuid().ToString(),
            Visual = new MemeVisual
            {
                Id = Guid.NewGuid().ToString(),
                Filename = "stuff",
                ContentHash = ""
            },
            ContentHash = ""
        };

        _context.Memes.Add(meme);
        await _context.SaveChangesAsync();

        // given
        var postVoteDTO = new PostVoteDTO
        {
            ElementIDs = [meme.Id],
            VoteNumber = 5,
            ExternalUserID = "",
            ExternalUserName = "",
        };

        // When
        var creationTask = await controller.PostVote(postVoteDTO);

        // Then
        var result = creationTask.Result;
        result.Should().BeEquivalentTo(controller.NotFound("User not found"));
    }
}
