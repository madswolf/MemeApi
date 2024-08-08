using FluentAssertions;
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
        var visual = new MemeVisual()
        {
            Id = Guid.NewGuid().ToString(),
            Filename = "stuff"
        };

        var toptext = new MemeText()
        {
            Id = Guid.NewGuid().ToString(),
            Text = "test",
            Position = MemeTextPosition.TopText,
        };

        var bottomtext = new MemeText()
        {
            Id = Guid.NewGuid().ToString(),
            Text = "test",
            Position = MemeTextPosition.BottomText,
        };

        var meme = new Meme() {
            Id = Guid.NewGuid().ToString(),
            Visual = visual,
            TopText = toptext,
            BottomText = bottomtext
        };

        _context.Visuals.Add(visual);
        _context.Texts.Add(bottomtext);
        _context.Texts.Add(toptext);
        _context.Memes.Add(meme);
        await _context.SaveChangesAsync();

        // When
        var result0 = await _textRepository.Delete(bottomtext.Id);
        var result1 = await _textRepository.Delete(toptext.Id);
        result0.Should().BeTrue();
        result1.Should().BeTrue();
        
        meme.BottomText.Should().BeNull();
        meme.TopText.Should().BeNull();
        
        var result2 = await _visualRepository.Delete(visual.Id);

        var memes = await _memeRepository.GetMemes(); 
        //Then
        result2.Should().BeTrue();
        memes.Should().BeEmpty();

    }
}
