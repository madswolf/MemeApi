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
    [
            "00 10 * * *",
            "00 20 * * *",
    ];

    public IServiceProvider Services { get; }
    public ConsumeScopedServiceHostedService(IServiceProvider services)
    {
        _nextRun = CalCulateNextRun();
        Services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(UntilNextExecution(), stoppingToken); // wait until next time

            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<IMemeOfTheDayService>();

                await scopedProcessingService.ExecuteAsync(stoppingToken);
            }

            _nextRun = CalCulateNextRun();
        }
    }

    private DateTime CalCulateNextRun()
    {
        var occurrences = Schedule.Select(s => CrontabSchedule.Parse(s).GetNextOccurrence(DateTime.Now)).ToList();
        occurrences.Sort(DateTime.Compare);
        return occurrences.First();
    }

    private int UntilNextExecution() => Math.Max(0, (int)_nextRun.Subtract(DateTime.Now).TotalMilliseconds);


    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await base.StopAsync(stoppingToken);
    }
}
