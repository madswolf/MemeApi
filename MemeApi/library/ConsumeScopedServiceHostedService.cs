using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MemeApi.library.repositories;
using MemeApi.library.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCrontab;
 
namespace MemeApi.library;
 
public class ConsumeScopedServiceHostedService : BackgroundService
{
    // Cron expressions are interpreted in Copenhagen wall-clock time (DST handled automatically).
    private static readonly List<string> Schedule =
    [
        //"* * * * *",
        "20 19 * * *",
    ];
 
    private static readonly TimeZoneInfo CopenhagenTz =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen");
 
    private const int CatchUpWindowMinutes = 60;
 
    public IServiceProvider Services { get; }
 
    public ConsumeScopedServiceHostedService(IServiceProvider services)
    {
        Services = services;
    }
 
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunIfMissedAsync(stoppingToken);
 
        var from = DateTime.UtcNow;
        while (!stoppingToken.IsCancellationRequested)
        {
            var nextRun = CalculateNextRun(from);
 
            var delay = nextRun - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, stoppingToken);
            }
 
            await InvokeMemeOfTheDayService(stoppingToken);
 
            var now = DateTime.UtcNow;
            from = now > nextRun ? now : nextRun;
        }
    }
 
    /// <summary>
    /// Catch-up check for when the app was down at the scheduled time:
    /// if a run was due in the last hour and no meme was created, run now.
    /// </summary>
    private async Task RunIfMissedAsync(CancellationToken stoppingToken)
    {
        var now = DateTime.UtcNow;
        var occurrencePast = CalculateNextRun(now.AddMinutes(-CatchUpWindowMinutes));
        var occurrenceNow = CalculateNextRun(now);
 
        if (occurrencePast == occurrenceNow)
        {
            return; 
        }
 
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<MemeRepository>();
 
        if (await repository.HasMemeCreatedInTimeSpan(occurrencePast, occurrenceNow))
        {
            Console.WriteLine($"Meme created in interval {occurrencePast:u} - {occurrenceNow:u}");
            return;
        }
 
        await RunMemeOfTheDayInScope(scope, stoppingToken);
    }
 
    private async Task InvokeMemeOfTheDayService(CancellationToken stoppingToken)
    {
        using var scope = Services.CreateScope();
        await RunMemeOfTheDayInScope(scope, stoppingToken);
    }
 
    private static async Task RunMemeOfTheDayInScope(IServiceScope scope, CancellationToken stoppingToken)
    {
        try
        {
            var service = scope.ServiceProvider.GetRequiredService<IMemeOfTheDayService>();
            await service.ExecuteAsync(stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
 
    /// <summary>
    /// Takes a UTC time and returns the next scheduled run as UTC.
    /// The cron schedule itself is evaluated in Copenhagen time.
    /// </summary>
    private static DateTime CalculateNextRun(DateTime utcTime)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(utcTime, CopenhagenTz);
 
        var nextLocal = Schedule
            .Select(s => CrontabSchedule.Parse(s).GetNextOccurrence(local))
            .Min();
 
        nextLocal = DateTime.SpecifyKind(nextLocal, DateTimeKind.Unspecified);
 
        if (CopenhagenTz.IsInvalidTime(nextLocal))
        {
            nextLocal = nextLocal.AddHours(1);
        }
 
        return TimeZoneInfo.ConvertTimeToUtc(nextLocal, CopenhagenTz);
    }
}
