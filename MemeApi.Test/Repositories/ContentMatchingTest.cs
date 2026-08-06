using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MemeApi.Models.DTO.Memes;
using MemeApi.Models.Entity;
using MemeApi.Models.Entity.Memes;
using MemeApi.Test.library;
using Xunit;

namespace MemeApi.Test.Repositories;

/// <summary>
/// Pins down the content matching / deduplication behavior that sits on top of the
/// content hashes. The shared decision logic lives in
/// TopicRepository.GetOrUpdateVotableIfExistsAndFilterTopics and is reached by every
/// votable type (Text, Visual, Meme) before it is created.
///
/// The intended model these tests document: identity is (content hash) and a votable
/// is reused across (owner + topic set). A re-post with no new topics returns the
/// existing votable; a re-post with new topics either extends the caller's own copy
/// or, for a different owner, creates a parallel copy with the same content hash.
/// </summary>
public class ContentMatchingTest(IntegrationTestFactory databaseFixture) : MemeTestBase(databaseFixture)
{
    private async Task<User> CreateUser()
    {
        var user = new User { Id = Guid.NewGuid().ToString() };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    private async Task<Topic> CreateUnrestrictedTopic()
    {
        var topic = new Topic
        {
            Id = Guid.NewGuid().ToString(),
            Name = "topic-" + Guid.NewGuid(),
            Description = "test",
            Owner = new User { Id = Guid.NewGuid().ToString() },
            HasRestrictedPosting = false,
            Moderators = []
        };
        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();
        return topic;
    }

    // ----- Text: the shared deduplication logic, exercised through the simplest votable -----

    [Fact]
    public async Task GIVEN_ExistingText_WHEN_CreatingIdenticalTextWithNoNewTopics_THEN_ReturnsExistingAndCreatesNoDuplicate()
    {
        var user = await CreateUser();

        var first = await _textRepository.CreateText("hello", MemeTextPosition.TopText, null, user.Id);
        var second = await _textRepository.CreateText("hello", MemeTextPosition.TopText, null, user.Id);

        second.Id.Should().Be(first.Id);
        _context.Texts.Count(t => t.ContentHash == first.ContentHash).Should().Be(1);
    }

    [Fact]
    public async Task GIVEN_ExistingText_WHEN_SameOwnerRepostsWithNewTopic_THEN_TopicAddedToExistingAndNoDuplicate()
    {
        var user = await CreateUser();
        var topicA = await CreateUnrestrictedTopic();
        var topicB = await CreateUnrestrictedTopic();

        var first = await _textRepository.CreateText("hello", MemeTextPosition.TopText, [topicA.Name], user.Id);
        var second = await _textRepository.CreateText("hello", MemeTextPosition.TopText, [topicA.Name, topicB.Name], user.Id);

        second.Id.Should().Be(first.Id);
        second.Topics.Select(t => t.Name).Should().Contain([topicA.Name, topicB.Name]);
        _context.Texts.Count(t => t.ContentHash == first.ContentHash).Should().Be(1);
    }

    [Fact]
    public async Task GIVEN_TextOwnedByOneUser_WHEN_DifferentUserCreatesIdenticalWithNewTopic_THEN_SeparateRowWithSameHash()
    {
        var ownerA = await CreateUser();
        var ownerB = await CreateUser();
        var topicA = await CreateUnrestrictedTopic();
        var topicB = await CreateUnrestrictedTopic();

        var first = await _textRepository.CreateText("hello", MemeTextPosition.TopText, [topicA.Name], ownerA.Id);
        var second = await _textRepository.CreateText("hello", MemeTextPosition.TopText, [topicB.Name], ownerB.Id);

        second.Id.Should().NotBe(first.Id);
        second.ContentHash.Should().Be(first.ContentHash);
        _context.Texts.Count(t => t.ContentHash == first.ContentHash).Should().Be(2);
    }

    [Fact]
    public async Task GIVEN_SameWords_WHEN_CreatedInTopAndBottomPosition_THEN_TreatedAsDifferentContent()
    {
        var user = await CreateUser();

        var top = await _textRepository.CreateText("foo", MemeTextPosition.TopText, null, user.Id);
        var bottom = await _textRepository.CreateText("foo", MemeTextPosition.BottomText, null, user.Id);

        bottom.Id.Should().NotBe(top.Id);
        bottom.ContentHash.Should().NotBe(top.ContentHash);
        _context.Texts.Count().Should().Be(2);
    }

    // ----- Visual: same dedup logic PLUS an extra filename layer unique to visuals -----

    [Fact]
    public async Task GIVEN_ExistingVisual_WHEN_CreatingIdenticalContentSameFilename_THEN_Deduplicated()
    {
        var user = await CreateUser();
        var file = CreateFormFile(50, "pic.png"); // reuse the same instance => identical bytes

        var first = await _visualRepository.CreateMemeVisual(file, "pic.png", null, user.Id);
        var second = await _visualRepository.CreateMemeVisual(file, "pic.png", null, user.Id);

        second.Id.Should().Be(first.Id);
        _context.Visuals.Count().Should().Be(1);
    }

    [Fact]
    public async Task GIVEN_ExistingVisual_WHEN_CreatingIdenticalContentDifferentFilename_THEN_DeduplicatedAndRequestedFilenameIgnored()
    {
        var user = await CreateUser();
        var file = CreateFormFile(50, "a.png");

        var first = await _visualRepository.CreateMemeVisual(file, "a.png", null, user.Id);
        var second = await _visualRepository.CreateMemeVisual(file, "b.png", null, user.Id);

        // Identity is the content hash, not the filename: the second filename is dropped.
        second.Id.Should().Be(first.Id);
        second.Filename.Should().Be("a.png");
        _context.Visuals.Count().Should().Be(1);
    }

    [Fact]
    public async Task GIVEN_ExistingVisual_WHEN_CreatingDifferentContentSameFilename_THEN_NewVisualGetsRandomizedFilename()
    {
        var user = await CreateUser();
        var file1 = CreateFormFile(50, "shared.png");
        var file2 = CreateFormFile(80, "shared.png"); // different size => different content hash

        var first = await _visualRepository.CreateMemeVisual(file1, "shared.png", null, user.Id);
        var second = await _visualRepository.CreateMemeVisual(file2, "shared.png", null, user.Id);

        second.Id.Should().NotBe(first.Id);
        second.ContentHash.Should().NotBe(first.ContentHash);
        // Filename collision with different content => a random prefix is added so the
        // existing stored file is not clobbered.
        second.Filename.Should().EndWith("shared.png");
        second.Filename.Should().NotBe("shared.png");
        _context.Visuals.Count().Should().Be(2);
    }

    // ----- Meme: the composed hash (visual + top + bottom) drives end-to-end dedup -----

    [Fact]
    public async Task GIVEN_ExistingMeme_WHEN_CreatingMemeWithIdenticalComponents_THEN_Deduplicated()
    {
        var user = await CreateUser();
        var file = CreateFormFile(50, "m.png");

        var first = await _memeRepository.CreateMeme(
            new MemeCreationDTO { VisualFile = file, FileName = "m.png", TopText = "top", BottomText = "bot", Topics = null },
            user.Id);
        var second = await _memeRepository.CreateMeme(
            new MemeCreationDTO { VisualFile = file, FileName = "m.png", TopText = "top", BottomText = "bot", Topics = null },
            user.Id);

        first.Should().NotBeNull();
        second.Should().NotBeNull();
        second!.Id.Should().Be(first!.Id);
        _context.Memes.Count().Should().Be(1);
    }
}
