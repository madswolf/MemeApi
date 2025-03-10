using System;
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