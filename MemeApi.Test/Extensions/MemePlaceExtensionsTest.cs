using System;
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
    public void GIVEN_MemePlace_AND_User_With_No_Submissions_WHEN_SubmittingChanges_THEN_SubmissionIsBumping()
    {
        // Given
        var user = new User { Id = Guid.NewGuid().ToString() };
        var time = DateTime.Parse("04-07-2025", CultureInfo.InvariantCulture);
        
        var place = new MemePlace
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            PlaceSubmissions = [],
            PriceHistory = [new PlacePixelPrice {PricePerPixel = 1}]
        };
        
        // When
        var isBumping = place.IsBumpingForUser(user, time);
        
        // Then
        isBumping.Should().Be(true);
    }
    
    [Fact]
    public void GIVEN_MemePlace_AND_User_With_SubmissionInPastWeek_WHEN_SubmittingChanges_THEN_SubmissionIsNotBumping()
    {
        // Given
        var user = new User { Id = Guid.NewGuid().ToString() };
        var time = DateTime.Parse("04-07-2025", CultureInfo.InvariantCulture);
        
        var place = new MemePlace
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            PlaceSubmissions = [],
            PriceHistory = [new PlacePixelPrice {PricePerPixel = 1}]
        };

        var previousSubmssion = new PlaceSubmission
        {
            Id = "1",
            Owner = user,
            OwnerId = user.Id,
            CreatedAt = time,
            Place = place,
        };
        place.PlaceSubmissions.Add(previousSubmssion);
        
        // When
        var isBumping = place.IsBumpingForUser(user, time.AddDays(2));
        
        // Then
        isBumping.Should().Be(false);
    }
    
    [Fact]
    public void GIVEN_MemePlace_AND_User_With_SubmissionInPastWeekButSubmissionIsInNewCalendarWeek_WHEN_SubmittingChanges_THEN_SubmissionIsBumping()
    {
        // Given
        var user = new User { Id = Guid.NewGuid().ToString() };
        var time = DateTime.Parse("04-07-2025", CultureInfo.InvariantCulture);
        
        var place = new MemePlace
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            PlaceSubmissions = [],
            PriceHistory = [new PlacePixelPrice {PricePerPixel = 1}]
        };

        var previousSubmssion = new PlaceSubmission
        {
            Id = "1",
            Owner = user,
            OwnerId = user.Id,
            CreatedAt = time.AddDays(-1),
            Place = place,
        };
        place.PlaceSubmissions.Add(previousSubmssion);
        
        // When
        var isBumping = place.IsBumpingForUser(user, time);
        
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
        price.Should().Be(-100);
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
        price.Should().Be(-99);
    }
    
    [Fact]
    public void GIVEN_MemePlace_AND_User_With_No_Submissions_WHEN_GettingSubmissionPrice_THEN_Price_Is_Discounted()
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
        price.Should().Be(86);
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
    
    [Fact]
    public void GIVEN_MemePlace_AND_User_With_No_Submissions_WHEN_GettingSubmissionPrice_THEN_Price_Is_Negative()
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
        var price = place.SubmissionPriceForUser(1, user, false);
        
        // Then
        price.Should().Be(-99);
    }
}