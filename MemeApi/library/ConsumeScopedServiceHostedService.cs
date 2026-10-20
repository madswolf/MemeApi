using System;
using System.Threading;
using System.Threading.Tasks;
using MemeApi.library.Services;
using MemeApi.Models.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MemeApi.library;

public class ConsumeScopedServiceHostedService(
    IServiceProvider services,
    ILogger<ConsumeScopedServiceHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PostIfDue(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to post meme of the day");
            }
            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    public async Task PostIfDue(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MemeContext>();
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        // Serialize posting and schedule edits, including across API instances.
        var schedule = await context.MemeOfTheDaySchedules
            .FromSqlRaw("SELECT * FROM \"MemeOfTheDaySchedules\" WHERE \"Id\" = 1 FOR UPDATE")
            .SingleAsync(cancellationToken);
        var now = DateTime.UtcNow;
        if (schedule.HasMissedWindow(now))
        {
            schedule.PlanNextRun(now);
        }
        else if (schedule.NextRunUtc <= now)
        {
            // shortcut: a crash after webhook delivery can duplicate a retry; add message reconciliation if required.
            await scope.ServiceProvider.GetRequiredService<IMemeOfTheDayService>().ExecuteAsync(cancellationToken);
            schedule.LastPostedDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(now, schedule.TimeZone));
            schedule.PlanNextRun(now);
        }
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
