using System.Threading;
using System.Threading.Tasks;

namespace MemeApi.library.Services;

public interface IMemeOfTheDayService
{
    public Task ExecuteAsync(CancellationToken stoppingToken);
}
