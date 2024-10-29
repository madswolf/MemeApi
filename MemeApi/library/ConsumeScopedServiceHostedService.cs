using MemeApi.library.repositories;
using MemeApi.library.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCrontab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MemeApi.library;

public class ConsumeScopedServiceHostedService : BackgroundService
{
    private DateTime _nextRun;
    private static readonly List<string> Schedule =
    [	    //"* * * * *",
            "30 13 * * *",
    ];

    public IServiceProvider Services { get; }
    public ConsumeScopedServiceHostedService(IServiceProvider services)
    {
        _nextRun = CalCulateNextRun(DateTime.UtcNow);
        Services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var occurencePast = CalCulateNextRun(DateTime.UtcNow.AddMinutes(-60));
            var occurenceNow = CalCulateNextRun(DateTime.UtcNow);
            using var scope = Services.CreateScope();
            if (occurencePast != occurenceNow)
            {
                var service = scope.ServiceProvider.GetRequiredService<MemeRepository>();
                var memeCreated = await service.HasMemeCreatedInTimeSpan(occurencePast, occurenceNow);
                if (!memeCreated)
                {
                    try
                    {
                        await InvokeMemeOfTheDayService(scope, stoppingToken);
                    } catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        throw;
                    }
                } else
                {
                    Console.WriteLine($"Meme created in interval {occurencePast} - {occurenceNow}");
                }
            }

            await Task.Delay(UntilNextExecution(), stoppingToken); // wait until next time
            await InvokeMemeOfTheDayService(scope, stoppingToken);

            _nextRun = CalCulateNextRun(DateTime.UtcNow);
        }
    }

    private static async Task InvokeMemeOfTheDayService(IServiceScope scope, CancellationToken stoppingToken)
    {
        var scopedProcessingService =
            scope.ServiceProvider
                .GetRequiredService<IMemeOfTheDayService>();

        await scopedProcessingService.ExecuteAsync(stoppingToken);
    }

    private DateTime CalCulateNextRun(DateTime time)
    {
        var occurrences = Schedule.Select(s => CrontabSchedule.Parse(s).GetNextOccurrence(time)).ToList();
        occurrences.Sort(DateTime.Compare);
        return occurrences.First();
    }

    private int UntilNextExecution() => Math.Max(0, (int)_nextRun.Subtract(DateTime.UtcNow).TotalMilliseconds);


    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await base.StopAsync(stoppingToken);
    }
}
