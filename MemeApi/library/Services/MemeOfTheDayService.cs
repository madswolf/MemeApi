namespace MemeApi.library.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

public class MemeOfTheDayBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Your daily logic here
            Console.WriteLine("Executing Daily Background Task");

            // Wait for 24 hours before the next execution
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}

