using System;
using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using MemeApi.Models.Entity.Places;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MemeApi.Test.Extensions;

public class MemePlaceExtensionsTest
{
    
    [Fact]
    public async Task GIVEN_MemePlace_AND_User_With_No_Submissions_WHEN_SubmittingChanges_THEN_SubmissionIsBumping()
    {
        // given
        var user = new User { Id = Guid.NewGuid().ToString() };
        
        var place = new MemePlace
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            PlaceSubmissions = [],
            PriceHistory = [new PlacePixelPrice(){PricePerPixel = 1}]
        };

        var submission = new PlaceSubmission
        {
            Owner = user,
            CreatedAt = DateTime.Parse("04-07-2025", CultureInfo.InvariantCulture),
            Place = place,
        };
        place.PlaceSubmissions.Add(submission);
        
        var isBumping = place.IsBumpingSubmission(submission);
        isBumping.Should().Be(true);
    }
    
    [Fact]
    public async Task GIVEN_MemePlace_AND_User_With_SubmissionInPastWeek_WHEN_SubmittingChanges_THEN_SubmissionIsNotBumping()
    {
        // given
        var user = new User { Id = Guid.NewGuid().ToString() };
        
        var place = new MemePlace
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            PlaceSubmissions = [],
            PriceHistory = [new PlacePixelPrice(){PricePerPixel = 1}]
        };

        var previousSubmssion = new PlaceSubmission
        {
            Id = "1",
            Owner = user,
            CreatedAt = DateTime.Parse("04-07-2025", CultureInfo.InvariantCulture),
            Place = place,
        };
        place.PlaceSubmissions.Add(previousSubmssion);
        
        var submission = new PlaceSubmission
        {
            Id = "2",
            Owner = user,
            CreatedAt = DateTime.Parse("04-07-2025", CultureInfo.InvariantCulture).AddDays(2),
            Place = place,
        };
        place.PlaceSubmissions.Add(submission);
        
        var isBumping = place.IsBumpingSubmission(submission);
        isBumping.Should().Be(false);
    }
    
    [Fact]
    public async Task GIVEN_MemePlace_AND_User_With_SubmissionInPastWeekButSubmissionIsInNewCalendarWeek_WHEN_SubmittingChanges_THEN_SubmissionIsBumping()
    {
        // given
        var user = new User { Id = Guid.NewGuid().ToString() };
        
        var place = new MemePlace
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            PlaceSubmissions = [],
            PriceHistory = [new PlacePixelPrice(){PricePerPixel = 1}]
        };

        var previousSubmssion = new PlaceSubmission
        {
            Id = "1",
            Owner = user,
            CreatedAt = DateTime.Parse("04-07-2025", CultureInfo.InvariantCulture).AddDays(-1),
            Place = place,
        };
        place.PlaceSubmissions.Add(previousSubmssion);
        
        var submission = new PlaceSubmission
        {
            Id = "2",
            Owner = user,
            CreatedAt = DateTime.Parse("04-07-2025", CultureInfo.InvariantCulture),
            Place = place,
        };
        place.PlaceSubmissions.Add(submission);
        
        var isBumping = place.IsBumpingSubmission(submission);
        isBumping.Should().Be(true);
    }
    
    [Fact]
    public async Task GIVEN_MemePlace_AND_User_With_No_Submissions_WHEN_GettingSubmissionPrice_THEN_Price_Is_Discounted()
    {
        // given
        var user = new User() { Id = Guid.NewGuid().ToString() };
        var place = new MemePlace()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            PlaceSubmissions = [],
            PriceHistory = [new PlacePixelPrice(){PricePerPixel = 1}]
        };

        var price = place.SubmissionPriceForUser(100, user);
        price.Should().Be(0);
    }
    
    [Fact]
    public async Task GIVEN_MemePlace_AND_User_With_Submission_1_day_Ago_WHEN_GettingSubmissionPrice_THEN_Price_Is_Partially_Discounted()
    {
        // given
        var user = new User() { Id = Guid.NewGuid().ToString() };
        var place = new MemePlace()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            PlaceSubmissions = [new PlaceSubmission(){ CreatedAt = DateTime.UtcNow.AddDays(-1), OwnerId = user.Id}],
            PriceHistory = [new PlacePixelPrice(){PricePerPixel = 1}]
        };

        var price = place.SubmissionPriceForUser(100, user);
        price.Should().Be(86);
    }
    
    [Fact]
    public async Task GIVEN_MemePlace_AND_User_With_Submission_Today_WHEN_GettingSubmissionPrice_THEN_Price_Is_Not_Discounted()
    {
        // given
        var user = new User() { Id = Guid.NewGuid().ToString() };
        var place = new MemePlace()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            PlaceSubmissions = [new PlaceSubmission(){ CreatedAt = DateTime.UtcNow, OwnerId = user.Id}],
            PriceHistory = [new PlacePixelPrice(){PricePerPixel = 1}]
        };

        var price = place.SubmissionPriceForUser(100, user);
        price.Should().Be(100);
    }
    
    [Fact]
    public async Task GIVEN_MemePlace_AND_User_With_No_Submissions_WHEN_GettingSubmissionPrice_THEN_Price_Is_Negative()
    {
        // given
        var user = new User() { Id = Guid.NewGuid().ToString() };
        var place = new MemePlace()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            PlaceSubmissions = [],
            PriceHistory = [new PlacePixelPrice(){PricePerPixel = 1}]
        };

        var price = place.SubmissionPriceForUser(1, user);
        price.Should().Be(-99);
    }
}