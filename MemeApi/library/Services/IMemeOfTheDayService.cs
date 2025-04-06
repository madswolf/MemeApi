using System.Threading;
using System.Threading.Tasks;

namespace MemeApi.library.Services;

internal interface IMemeOfTheDayService
{
    public Task ExecuteAsync(CancellationToken stoppingToken);
}