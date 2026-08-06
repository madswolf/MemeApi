using System;
using FluentAssertions;
using MemeApi.library.Extensions;
using MemeApi.Models.Entity.Memes;
using Xunit;

namespace MemeApi.Test.Extensions;

/// <summary>
/// Pins down how content hashes are COMPUTED (the primitives in Extensions.cs).
/// These are pure and need no database. The matching/deduplication behavior that
/// is built on top of these hashes is covered in ContentMatchingTest.
/// </summary>
public class ContentHashExtensionsTest
{
    private static MemeVisual Visual(string contentHash) =>
        new() { Id = Guid.NewGuid().ToString(), Filename = "f", ContentHash = contentHash };

    private static MemeText Text(string contentHash) =>
        new() { Id = Guid.NewGuid().ToString(), Text = "t", ContentHash = contentHash };

    [Fact]
    public void GIVEN_SameBytes_WHEN_Hashing_THEN_HashIsDeterministic()
    {
        var a = new byte[] { 1, 2, 3, 4 };
        var b = new byte[] { 1, 2, 3, 4 };

        a.ToContentHash().Should().Be(b.ToContentHash());
    }

    [Fact]
    public void GIVEN_DifferentBytes_WHEN_Hashing_THEN_HashesDiffer()
    {
        var a = new byte[] { 1, 2, 3, 4 };
        var b = new byte[] { 1, 2, 3, 5 };

        a.ToContentHash().Should().NotBe(b.ToContentHash());
    }

    [Fact]
    public void GIVEN_Bytes_WHEN_Hashing_THEN_OutputIsUppercaseDashSeparatedHex()
    {
        // Documents the wire format produced by BitConverter.ToString(SHA-256(...)).
        // If this ever changes, every persisted ContentHash silently stops matching.
        var hash = new byte[] { 0, 255, 16 }.ToContentHash();

        hash.Should().MatchRegex("^([0-9A-F]{2}-)*[0-9A-F]{2}$");
    }

    [Fact]
    public void GIVEN_NullString_WHEN_Hashing_THEN_ReturnsEmptyString()
    {
        // Note: null hashes to "" rather than to the hash of an empty byte array.
        ((string?)null).ToContentHash().Should().Be("");
    }

    [Fact]
    public void GIVEN_String_WHEN_Hashing_THEN_EqualsHashOfItsUtf8Bytes()
    {
        "héllo".ToContentHash()
            .Should().Be(System.Text.Encoding.UTF8.GetBytes("héllo").ToContentHash());
    }

    [Fact]
    public void GIVEN_SameTextDifferentPosition_WHEN_HashingTextComponent_THEN_HashesDiffer()
    {
        // MemeText hashes "text + position" (see TextRepository.CreateText), so the
        // SAME words in the top vs. bottom slot are treated as different content.
        var top = ("foo" + MemeTextPosition.TopText).ToContentHash();
        var bottom = ("foo" + MemeTextPosition.BottomText).ToContentHash();

        top.Should().NotBe(bottom);
    }

    [Fact]
    public void GIVEN_MemeWithSameComponents_WHEN_Hashing_THEN_HashIsDeterministic()
    {
        var a = new Meme { Visual = Visual("V"), TopText = Text("T"), BottomText = Text("B"), ContentHash = "" };
        var b = new Meme { Visual = Visual("V"), TopText = Text("T"), BottomText = Text("B"), ContentHash = "" };

        a.ToContentHash().Should().Be(b.ToContentHash());
    }

    [Fact]
    public void GIVEN_Meme_WHEN_OneComponentHashChanges_THEN_MemeHashChanges()
    {
        var original = new Meme { Visual = Visual("V"), TopText = Text("T"), BottomText = Text("B"), ContentHash = "" };
        var changed = new Meme { Visual = Visual("V"), TopText = Text("DIFFERENT"), BottomText = Text("B"), ContentHash = "" };

        original.ToContentHash().Should().NotBe(changed.ToContentHash());
    }

    [Fact]
    public void GIVEN_MemeWithNullTexts_WHEN_Hashing_THEN_DoesNotThrow_AND_DiffersFromMemeWithTexts()
    {
        var withoutTexts = new Meme { Visual = Visual("V"), TopText = null, BottomText = null, ContentHash = "" };
        var withTexts = new Meme { Visual = Visual("V"), TopText = Text("T"), BottomText = Text("B"), ContentHash = "" };

        withoutTexts.Invoking(m => m.ToContentHash()).Should().NotThrow();
        withoutTexts.ToContentHash().Should().NotBe(withTexts.ToContentHash());
    }

    [Fact]
    public void GIVEN_TwoMemes_SameTextInTopVsBottom_WHEN_Hashing_THEN_HashesDiffer()
    {
        // Same visual, same word, but placed top-only vs. bottom-only. Because the
        // position is baked into each text's hash, the two memes are not duplicates.
        var topOnly = new Meme
        {
            Visual = Visual("V"),
            TopText = Text(("foo" + MemeTextPosition.TopText).ToContentHash()),
            BottomText = null,
            ContentHash = ""
        };
        var bottomOnly = new Meme
        {
            Visual = Visual("V"),
            TopText = null,
            BottomText = Text(("foo" + MemeTextPosition.BottomText).ToContentHash()),
            ContentHash = ""
        };

        topOnly.ToContentHash().Should().NotBe(bottomOnly.ToContentHash());
    }

    [Fact]
    public void GIVEN_MemeWithNullVisual_WHEN_Hashing_THEN_Throws()
    {
        // Documents a sharp edge: Visual is dereferenced unconditionally (unlike the
        // texts), so a Meme without a Visual throws rather than producing a hash.
        var meme = new Meme { Visual = null, TopText = null, BottomText = null, ContentHash = "" };

        meme.Invoking(m => m.ToContentHash()).Should().Throw<NullReferenceException>();
    }
}
