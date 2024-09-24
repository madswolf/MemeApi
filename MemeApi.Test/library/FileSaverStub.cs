using MemeApi.library.Services.Files;
using System.Threading.Tasks;

namespace MemeApi.Test.library;

internal class FileSaverStub : IFileSaver
{
    public Task SaveFile(byte[] file, string path, string fileName, string contentType)
    {
        return Task.CompletedTask;
    }
}
