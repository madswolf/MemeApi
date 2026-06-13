using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using MemeApi.library.Extensions;
using MemeApi.Models.Entity;
using MemeApi.Models.Entity.Places;
using Xunit;

namespace MemeApi.Test.Extensions;

public class MemePlaceExtensionsTest
{

    [Fact]
    public void GIVEN_User_With_No_Submissions_WHEN_SubmittingChanges_THEN_SubmissionIsBumping()
    {
        // Given
        var user = new User { Id = Guid.NewGuid().ToString(), PlaceSubmissions = [] };
        var time = DateTime.Parse("04-07-2025", CultureInfo.InvariantCulture);

        // When
        var isBumping = user.IsFirstPlaceSubmissionThisWeek(time);

        // Then
        isBumping.Should().Be(true);
    }

    [Fact]
    public void GIVEN_User_With_SubmissionInCurrentWeek_WHEN_SubmittingChanges_THEN_SubmissionIsNotBumping()
    {
        // Given
        var time = DateTime.Parse("04-07-2025", CultureInfo.InvariantCulture);
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            PlaceSubmissions = [new PlaceSubmission { Id = "1", CreatedAt = time }]
        };

        // When
        var isBumping = user.IsFirstPlaceSubmissionThisWeek(time.AddDays(2));

        // Then
        isBumping.Should().Be(false);
    }

    [Fact]
    public void GIVEN_User_With_SubmissionInPreviousCalendarWeek_WHEN_SubmittingChanges_THEN_SubmissionIsBumping()
    {
        // Given
        var time = DateTime.Parse("04-07-2025", CultureInfo.InvariantCulture);
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            PlaceSubmissions = [new PlaceSubmission { Id = "1", CreatedAt = time.AddDays(-1) }]
        };

        // When
        var isBumping = user.IsFirstPlaceSubmissionThisWeek(time);

        // Then
        isBumping.Should().Be(true);
    }

    [Fact]
    public void GIVEN_MemePlace_AND_User_With_No_Submissions_And_Is_Bumping_WHEN_GettingSubmissionPrice_THEN_Price_Is_Discounted_and_User_Gains_Dubloons()
    {
        // Given
        var user = new User { Id = Guid.NewGuid().ToString() };
        var place = new MemePlace
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            PlaceSubmissions = [],
            PriceHistory = [new PlacePixelPrice {PricePerPixel = 1}]
        };

        // When
        var price = place.SubmissionPriceForUser(1, user, true);

        // Then
        price.Should().Be(-700);
    }

    [Fact]
    public void GIVEN_MemePlace_AND_User_With_No_Submissions_And_Is_Bumping_And_Over_Free_Pixel_ThreshHold_WHEN_GettingSubmissionPrice_THEN_Price_Is_Discounted_and_User_Gains_Almost_Full_Dubloons()
    {
        // Given
        var user = new User { Id = Guid.NewGuid().ToString() };
        var place = new MemePlace
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            PlaceSubmissions = [],
            PriceHistory = [new PlacePixelPrice {PricePerPixel = 1}]
        };

        // When
        var price = place.SubmissionPriceForUser(201, user, true);

        // Then
        price.Should().Be(-699);
    }

    [Fact]
    public void GIVEN_MemePlace_AND_User_With_No_Submissions_And_Bumping_WHEN_GettingSubmissionPrice_THEN_Price_Is_Discounted_And_Dubloons_Earned()
    {
        // Given
        var user = new User { Id = Guid.NewGuid().ToString() };
        var place = new MemePlace
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            PlaceSubmissions = [],
            PriceHistory = [new PlacePixelPrice {PricePerPixel = 1}]
        };

        // When
        var price = place.SubmissionPriceForUser(100, user, false);

        // Then
        price.Should().Be(0);
    }
    
    [Fact]
    public void GIVEN_MemePlace_AND_User_With_No_Submissions_And_Not_Bumping_WHEN_GettingSubmissionPrice_THEN_Price_Is_Discounted_But_Clamped_To_0()
    {
        // Given
        var user = new User { Id = Guid.NewGuid().ToString() };
        var place = new MemePlace
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            PlaceSubmissions = [],
            PriceHistory = [new PlacePixelPrice {PricePerPixel = 1}]
        };

        // When
        var price = place.SubmissionPriceForUser(100, user, false);

        // Then
        price.Should().Be(0);
    }

    [Fact]
    public void GIVEN_MemePlace_AND_User_With_Submission_1_day_Ago_WHEN_GettingSubmissionPrice_THEN_Price_Is_Partially_Discounted()
    {
        // Given
        var user = new User { Id = Guid.NewGuid().ToString() };
        var place = new MemePlace
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            PlaceSubmissions = [new PlaceSubmission { CreatedAt = DateTime.UtcNow.AddDays(-1), OwnerId = user.Id}],
            PriceHistory = [new PlacePixelPrice {PricePerPixel = 1}]
        };

        // When
        var price = place.SubmissionPriceForUser(100, user, false);

        // Then
        price.Should().Be(0);
    }

    [Fact]
    public void GIVEN_MemePlace_AND_User_With_Submission_Today_WHEN_GettingSubmissionPrice_THEN_Price_Is_Not_Discounted()
    {
        // Given
        var user = new User { Id = Guid.NewGuid().ToString() };
        var place = new MemePlace
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            PlaceSubmissions = [new PlaceSubmission { CreatedAt = DateTime.UtcNow, OwnerId = user.Id}],
            PriceHistory = [new PlacePixelPrice {PricePerPixel = 1}]
        };

        // When
        var price = place.SubmissionPriceForUser(100, user, false);

        // Then
        price.Should().Be(100);
    }
}
