using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MemeApi.library;
using MemeApi.library.Services;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using MemeApi.Test.library;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace MemeApi.Test.Controllers;

public class MemeOfTheDayControllerTest(IntegrationTestFactory fixture) : MemeTestBase(fixture)
{
    [Theory]
    [InlineData(null)]
    [InlineData("wrong-secret")]
    public async Task ScheduleCannotBeChangedWithoutBotAuthentication(string? secret)
    {
        using var client = _fixture.CreateClient();
        if (secret != null) client.DefaultRequestHeaders.Add("Bot_Secret", secret);
        var response = await client.PutAsJsonAsync("/memeoftheday/schedule", new { start = "13:00" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("24:00", null)]
    [InlineData("13:00", "12:00")]
    [InlineData("13:00", "")]
    public async Task InvalidScheduleIsRejected(string start, string? end)
    {
        using var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Add("Bot_Secret", "lol");
        var response = await client.PutAsJsonAsync("/memeoftheday/schedule", new { start, end });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(null, 780)]
    [InlineData("15:00", 900)]
    public async Task ScheduleAndRandomChoiceArePersisted(string? end, int expectedEnd)
    {
        _context.MemeOfTheDaySchedules.Add(new MemeOfTheDaySchedule());
        await _context.SaveChangesAsync();
        using var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Add("Bot_Secret", "lol");
        var response = await client.PutAsJsonAsync("/memeoftheday/schedule", new { start = "13:00", end });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MemeContext>();
        var saved = await context.MemeOfTheDaySchedules.SingleAsync();
        Assert.Equal(780, saved.StartMinute);
        Assert.Equal(expectedEnd, saved.EndMinute);
        Assert.NotNull(saved.NextRunUtc);
        var nextRun = saved.NextRunUtc;
        await CreateScheduler(scope.ServiceProvider).PostIfDue(CancellationToken.None);
        await context.Entry(saved).ReloadAsync();
        Assert.Equal(nextRun, saved.NextRunUtc);
    }

    [Fact]
    public async Task ConcurrentSchedulerRunsPostOnlyOnceAndRestartDoesNotRepeat()
    {
        await SeedDueSchedule();
        var posting = new Mock<IMemeOfTheDayService>();
        posting.Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        using var services = CreatePostingServices(posting.Object);
        await Task.WhenAll(
            CreateScheduler(services).PostIfDue(CancellationToken.None),
            CreateScheduler(services).PostIfDue(CancellationToken.None));
        await CreateScheduler(services).PostIfDue(CancellationToken.None);
        posting.Verify(x => x.ExecuteAsync(It.IsAny<CancellationToken>()), Times.Once);
        _context.ChangeTracker.Clear();
        var saved = await _context.MemeOfTheDaySchedules.SingleAsync();
        Assert.NotNull(saved.LastPostedDate);
        Assert.True(saved.NextRunUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task FailedPostKeepsPersistedChoiceForRetry()
    {
        var due = await SeedDueSchedule();
        var posting = new Mock<IMemeOfTheDayService>();
        posting.Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Webhook failed"));
        using var services = CreatePostingServices(posting.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateScheduler(services).PostIfDue(CancellationToken.None));
        _context.ChangeTracker.Clear();
        var saved = await _context.MemeOfTheDaySchedules.SingleAsync();
        Assert.Null(saved.LastPostedDate);
        Assert.Equal(due, saved.NextRunUtc);
    }

    private async Task<DateTime> SeedDueSchedule()
    {
        var due = DateTime.UtcNow.AddSeconds(-1);
        _context.MemeOfTheDaySchedules.Add(new MemeOfTheDaySchedule
        {
            StartMinute = 0, EndMinute = 1439, NextRunUtc = due
        });
        await _context.SaveChangesAsync();
        // PostgreSQL timestamps have microsecond precision.
        await _context.Entry(await _context.MemeOfTheDaySchedules.SingleAsync()).ReloadAsync();
        return (await _context.MemeOfTheDaySchedules.SingleAsync()).NextRunUtc!.Value;
    }

    private ServiceProvider CreatePostingServices(IMemeOfTheDayService posting)
    {
        var services = new ServiceCollection();
        services.AddSingleton(_settings);
        services.AddSingleton(posting);
        services.AddDbContext<MemeContext>(options => options.UseNpgsql(_context.Database.GetConnectionString()));
        return services.BuildServiceProvider();
    }

    private static ConsumeScopedServiceHostedService CreateScheduler(IServiceProvider services) =>
        new(services, NullLogger<ConsumeScopedServiceHostedService>.Instance);
}
