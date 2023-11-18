using FluentAssertions;
using MemeApi.Models.Entity;
using MemeApi.Test.library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MemeApi.Test.Repositories
{
    public class TopicRepositoryTest : MemeTestBase
    {
        [Fact]
        public async Task GIVEN_Votable_WHEN_OwnerOfTopicDeleting_THEN_VotableDeleted()
        {
            // given
            var user = new User() { Id = Guid.NewGuid().ToString() };
            var topic = new Topic { Id = Guid.NewGuid().ToString(), Name = "Test", Owner = user};
            var votable = new Votable {Id = Guid.NewGuid().ToString(), Topics = new List<Topic>() { topic } };
            _context.Votables.Add(votable);
            _context.SaveChanges();

            // When
            var result = await _votableRepository.DeleteVotable(votable, user);

            // Then
            result.Should().BeTrue();
            _context.Votables.Count().Should().Be(0);
        }

        [Fact]
        public async Task GIVEN_Votable_WHEN_ModeratorOfTopicDeleting_THEN_VotableDeleted()
        {
            // given
            var user = new User() { Id = Guid.NewGuid().ToString() };
            var topic = new Topic { Id = Guid.NewGuid().ToString(), Name = "Test", Moderators = new List<User> { user } };
            var votable = new Votable {
                Id = Guid.NewGuid().ToString(),
                Topics = new List<Topic>() { topic }
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
            var user = new User() { Id = Guid.NewGuid().ToString() };
            var topic = new Topic { Id = Guid.NewGuid().ToString(), Name = "Test" , Owner = new User() { Id = Guid.NewGuid().ToString() }, Moderators = new List<User>()};
            var votable = new Votable {Id = Guid.NewGuid().ToString(), Topics = new List<Topic>() { topic }};
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
            var user = new User() { Id = Guid.NewGuid().ToString() };
            var topic = new Topic { Id = Guid.NewGuid().ToString(), Name = "Test", Owner = user };
            var topic2 = new Topic { Id = Guid.NewGuid().ToString(), Name = "Test2", Owner = new User() { Id = Guid.NewGuid().ToString() }, Moderators = new List<User>() };
            var votable = new Votable { Id = Guid.NewGuid().ToString(), Topics = new List<Topic>() { topic, topic2 } };
            _context.Votables.Add(votable);
            _context.SaveChanges();

            // When
            var result = await _votableRepository.DeleteVotable(votable, user);
            var resultVotable = await _votableRepository.GetVotable(votable.Id);
            // Then
            result.Should().BeTrue();
            resultVotable.Topics.Count().Should().Be(1);
            resultVotable.Topics.Should().NotContain(topic);
        }

        [Fact]
        public async Task GIVEN_VotableInMultipleTopics_WHEN_ModeratorDeleting_THEN_VotableNotDeletedButRemovedFromTopic()
        {
            // given
            var user = new User() { Id = Guid.NewGuid().ToString() };
            var topic = new Topic { Id = Guid.NewGuid().ToString(), Name = "Test", Moderators = new List<User> { user } };
            var topic2 = new Topic { Id = Guid.NewGuid().ToString(), Name = "Test2", Owner = new User() { Id = Guid.NewGuid().ToString() }, Moderators = new List<User>() };
            var votable = new Votable { Id = Guid.NewGuid().ToString(), Topics = new List<Topic>() { topic, topic2 } };
            _context.Votables.Add(votable);
            _context.SaveChanges();

            // When
            var result = await _votableRepository.DeleteVotable(votable, user);
            var resultVotable = await _votableRepository.GetVotable(votable.Id);
            // Then
            result.Should().BeTrue();
            resultVotable.Topics.Count().Should().Be(1);
            resultVotable.Topics.Should().NotContain(topic);
        }
    }
}
