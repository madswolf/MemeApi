using FluentAssertions;
using MemeApi.library.Extensions;
using MemeApi.Models.Entity;
using MemeApi.Test.library;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MemeApi.Test.Repositories;

public class VotableRepositoryTest(IntegrationTestFactory databaseFixture) : MemeTestBase(databaseFixture)
{
    [Fact]
    public async Task GIVEN_ExistingMemeWithVotable_WHEN_DeletingVotables_THEN_VotablesDeleted()
    {
        // given

        var meme = new Meme() {
            Id = Guid.NewGuid().ToString(),
            Visual = new MemeVisual()
            {
                Id = Guid.NewGuid().ToString(),
                Filename = "stuff"
            },
            TopText = new MemeText()
            {
                Id = Guid.NewGuid().ToString(),
                Text = "test",
                Position = MemeTextPosition.TopText,
            },
            BottomText = new MemeText()
            {
                Id = Guid.NewGuid().ToString(),
                Text = "test2",
                Position = MemeTextPosition.BottomText,
            }
        };

        _context.Memes.Add(meme);
        await _context.SaveChangesAsync();

        // When
        var result0 = await _textRepository.Delete(meme.BottomText.Id);
        var result1 = await _textRepository.Delete(meme.TopText.Id);
        result0.Should().BeTrue();
        result1.Should().BeTrue();
        
        meme.BottomText.Should().BeNull();
        meme.TopText.Should().BeNull();
        
        var result2 = await _visualRepository.Delete(meme.Visual.Id);

        var memes = await _memeRepository.GetMemes(); 
        //Then
        result2.Should().BeTrue();
        memes.Should().BeEmpty();

    }

    [Fact]
    public async Task GIVEN_ExistingNonMemeOfTheDayMeme_WHEN_Voting_THEN_DubloonEventNotCreatedRewarding100Dubloons()
    {
        var defaultTopic = await _topicRepository.GetTopicByName(_settings.GetDefaultTopicName());
        var meme = new Meme()
        {
            Id = Guid.NewGuid().ToString(),
            Visual = new MemeVisual()
            {
                Id = Guid.NewGuid().ToString(),
                Filename = "stuff"
            },
            Topics = [defaultTopic]
        };

        _context.Memes.Add(meme);
        await _context.SaveChangesAsync();

        var user = new User { Id = Guid.NewGuid().ToString() };

        var vote = new Vote
        {
            Id = Guid.NewGuid().ToString(),
            User = user,
            Element = meme,
        };

        // When
        await _votableRepository.CreateVote(vote);

        var resultUser = await _userRepository.GetUser(user.Id, includeDubloons: true);

        //Then
        resultUser.Should().NotBeNull();
        resultUser?.DubloonEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task GIVEN_ExistingNonMemeVotableInMemeOfTheDayTopic_WHEN_Voting_THEN_DubloonEventNotCreated()
    {
        var defaultTopic = await _topicRepository.GetTopicByName(_settings.GetDefaultTopicName());

        var visual = new MemeVisual()
        {
            Id = Guid.NewGuid().ToString(),
            Filename = "stuff",
            Topics = [defaultTopic]
        };
            
        _context.Visuals.Add(visual);
        await _context.SaveChangesAsync();

        var user = new User { Id = Guid.NewGuid().ToString() };

        var vote = new Vote
        {
            Id = Guid.NewGuid().ToString(),
            User = user,
            Element = visual,
        };

        // When
        await _votableRepository.CreateVote(vote);

        var resultUser = await _userRepository.GetUser(user.Id, includeDubloons: true);

        //Then
        resultUser.Should().NotBeNull();
        resultUser?.DubloonEvents.Should().BeEmpty();
    }


    [Theory]
    [InlineData(0, 100.1, 99.9)]
    [InlineData(-60*60, 100.0, 99.9)]
    [InlineData(-60*60*2, 75.0, 74.9)]
    [InlineData(-60*60*3, 10.0, 9.8)]
    [InlineData(-(60 * 59 * 3 + 24 * 60.0*60), 6.7, 6.46)]
    public async Task GIVEN_ExistingMemeOfTheDayMeme_WHEN_VotingAfterAMinute_THEN_DubloonEventCreatedRewarding10Dubloons(double seconds, double upperBound, double lowerBound)
    {
        var memeOfTheDayTopic = await _topicRepository.GetTopicByName(_settings.GetMemeOfTheDayTopicName());
        var meme = new Meme()
        {
            Id = Guid.NewGuid().ToString(),
            Visual = new MemeVisual()
            {
                Id = Guid.NewGuid().ToString(),
                Filename = "stuff"
            },
            Topics = [memeOfTheDayTopic]
        };

        _context.Memes.Add(meme);
        await _context.SaveChangesAsync();

        var user = new User { Id = Guid.NewGuid().ToString() };
        meme.CreatedAt = DateTime.UtcNow.AddSeconds(seconds);
        var vote = new Vote
        {
            Id = Guid.NewGuid().ToString(),
            User = user,
            Element = meme,
        };

        // When
        await _votableRepository.CreateVote(vote);

        var resultUser = await _userRepository.GetUser(user.Id, includeDubloons: true);

        //Then
        resultUser.Should().NotBeNull();
        resultUser?.DubloonEvents.Should().NotBeEmpty();
        resultUser?.DubloonEvents.CountDubloons().Should().BeLessThan(upperBound);
        resultUser?.DubloonEvents.CountDubloons().Should().BeGreaterThan(lowerBound);
    }
}
