using FluentAssertions;
using MemeApi.library.repositories;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using MemeApi.Test.library;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MemeApi.Test.Repositories;

public class TopicRepositoryTest(IntegrationTestFactory databaseFixture) : MemeTestBase(databaseFixture)
{
    [Fact (Skip = "run alone because of error")]
    public async Task GIVEN_ExistingTopic_WHEN_CreatingTopicWithSameName_THEN_TopicNotCreated()
    {
        // given
        var owner = new User() { Id = Guid.NewGuid().ToString() };
        _context.Users.Add(owner);
        _context.SaveChanges();
    
        // When
        Task<TopicDTO?> task(TopicRepository x) => x.CreateTopic(
            new TopicCreationDTO
            {
                TopicName = _settings.GetDefaultTopicName(),
                Description = "test"
            },
           owner.Id);
    
        //Then
        await _topicRepository.Invoking(task).Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task GIVEN_Votable_WHEN_OwnerOfTopicDeleting_THEN_VotableDeleted()
    {
        // given
        var owner = new User() { Id = Guid.NewGuid().ToString() };
        var topic = new Topic
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            Description = "test",
            Owner = owner,
            HasRestrictedPosting = true,
            Moderators = []
        };
        var votable = new Votable { Id = Guid.NewGuid().ToString(), Topics = [topic] };
        _context.Votables.Add(votable);
        _context.SaveChanges();
        // When
        var result = await _votableRepository.DeleteVotable(votable, owner);

        // Then
        result.Should().BeTrue();
        _context.Votables.Count().Should().Be(0);
    }

    [Fact]
    public async Task GIVEN_TopicWithRestrictedPosting_WHEN_NotAllowedUserPosts_THEN_PostDoesNotHaveTopic()
    {
        // given
        var owner = new User() { Id = Guid.NewGuid().ToString() };
        var user = new User() { Id = Guid.NewGuid().ToString() };
        var topic = new Topic {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            Description = "test",
            Owner = owner,
            HasRestrictedPosting = true,
            Moderators = []
        };
        var text = "";

        _context.Users.Add(owner);
        _context.Users.Add(user);
        _context.Topics.Add(topic);
        _context.SaveChanges();
        
        // When
        var result = await _textRepository.CreateText(text, MemeTextPosition.TopText, [topic.Name], user.Id);

        // Then
        result.Topics.Should().NotContain(topic);
        result.Topics.Count.Should().Be(1);
    }

    [Fact]
    public async Task GIVEN_TopicWithRestrictedPosting_WHEN_AllowedUserPosts_THEN_PostDoesHaveTopic()
    {
        // given
        var owner = new User() { Id = Guid.NewGuid().ToString() };
        var user = new User() { Id = Guid.NewGuid().ToString() };
        var topic = new Topic
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            Description = "test",
            Owner = owner,
            HasRestrictedPosting = true,
            Moderators = [user]
        };
        var text = "";

        _context.Users.Add(user);
        _context.Topics.Add(topic);
        _context.SaveChanges();

        // When
        var result = await _textRepository.CreateText(text, MemeTextPosition.TopText, [topic.Name], user.Id);

        // Then
        result.Topics.Should().Contain(topic);
        result.Topics.Count.Should().Be(1);
    }

    [Fact]
    public async Task GIVEN_DefaultTopic_WHEN_AnonymousPosts_THEN_PostDoesHaveDefaultTopic()
    {
        var defaultTopic = await _topicRepository.GetDefaultTopic();

        // When
        var result = await _textRepository.CreateText("test", MemeTextPosition.TopText);

        // Then
        result.Topics.Should().Contain(defaultTopic);
        result.Topics.Count.Should().Be(1);
    }

    [Fact]
    public async Task GIVEN_DefaultTopic_WHEN_UserPosts_THEN_PostDoesHaveDefaultTopic()
    {
        var defaultTopic = await _topicRepository.GetDefaultTopic();
        var user = new User() { Id = Guid.NewGuid().ToString() };

        _context.Users.Add(user);
        _context.SaveChanges();

        // When
        var result = await _textRepository.CreateText("test", MemeTextPosition.TopText, userId: user.Id);

        // Then
        result.Topics.Should().Contain(defaultTopic);
        result.Topics.Count.Should().Be(1);
    }


    [Fact]
    public async Task GIVEN_Votable_WHEN_ModeratorOfTopicDeleting_THEN_VotableDeleted()
    {
        // given
        var owner = new User() { Id = Guid.NewGuid().ToString() };
        var user = new User() { Id = Guid.NewGuid().ToString() };
        var topic = new Topic
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            Description = "test",
            Owner = owner,
            HasRestrictedPosting = true,
            Moderators = [user]
        };
        var votable = new Votable {
            Id = Guid.NewGuid().ToString(),
            Topics = [topic]
        };
        _context.Votables.Add(votable);
        _context.SaveChanges();

        // When
        var result = await _votableRepository.DeleteVotable(votable, user);

        // Then
        result.Should().BeTrue();
        _context.Votables.Count().Should().Be(0);
    }

    [Fact]
    public async Task GIVEN_Votable_WHEN_NonModeratorOrOwnerOfTopicDeleting_THEN_VotableNotDeleted()
    {
        // given
        var owner = new User() { Id = Guid.NewGuid().ToString() };
        var user = new User() { Id = Guid.NewGuid().ToString() };
        var topic = new Topic
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            Description = "test",
            Owner = owner,
            HasRestrictedPosting = true,
            Moderators = []
        };
        var votable = new Votable { Id = Guid.NewGuid().ToString(), Topics = [topic] };
        _context.Votables.Add(votable);
        _context.SaveChanges();

        // When
        var result = await _votableRepository.DeleteVotable(votable, user);

        // Then
        result.Should().BeFalse();
        _context.Votables.Count().Should().Be(1);
    }

    [Fact]
    public async Task GIVEN_VotableInMultipleTopics_WHEN_OwnerDeleting_THEN_VotableNotDeletedButRemovedFromTopic()
    {
        // given
        var owner = new User() { Id = Guid.NewGuid().ToString() };
        var topic = new Topic
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            Description = "test",
            Owner = owner,
            HasRestrictedPosting = true,
            Moderators = []
        };
        var topic2 = new Topic
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test2",
            Description = "test",
            Owner = new User() { Id = Guid.NewGuid().ToString() },
            HasRestrictedPosting = true,
            Moderators = []
        };
        var votable = new Votable { Id = Guid.NewGuid().ToString(), Topics = [topic, topic2] };
        _context.Votables.Add(votable);
        _context.SaveChanges();

        // When
        var result = await _votableRepository.DeleteVotable(votable, owner);
        var resultVotable = await _votableRepository.GetVotable(votable.Id);
        
        // Then
        result.Should().BeTrue();
        resultVotable?.Topics.Count.Should().Be(1);
        resultVotable?.Topics.Should().NotContain(topic);
    }

    [Fact]
    public async Task GIVEN_VotableInMultipleTopics_WHEN_ModeratorDeleting_THEN_VotableNotDeletedButRemovedFromTopic()
    {
        // given
        var owner = new User() { Id = Guid.NewGuid().ToString() };
        var user = new User() { Id = Guid.NewGuid().ToString() };
        var topic = new Topic
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            Description = "test",
            Owner = owner,
            HasRestrictedPosting = true,
            Moderators = [user]
        };
        var topic2 = new Topic
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test2",
            Description = "test",
            Owner = new User() { Id = Guid.NewGuid().ToString() },
            HasRestrictedPosting = true,
            Moderators = []
        };
        var votable = new Votable { Id = Guid.NewGuid().ToString(), Topics = [topic, topic2] };
        _context.Votables.Add(votable);
        _context.SaveChanges();

        // When
        var result = await _votableRepository.DeleteVotable(votable, user);
        var resultVotable = await _votableRepository.GetVotable(votable.Id);
        // Then
        result.Should().BeTrue();
        resultVotable?.Topics.Count.Should().Be(1);
        resultVotable?.Topics.Should().NotContain(topic);
    }
}
