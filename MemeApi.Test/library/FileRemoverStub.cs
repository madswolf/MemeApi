using System.Threading.Tasks;
using MemeApi.library.Services.Files;

namespace MemeApi.Test.library;

internal class FileRemoverStub : IFileRemover
{
    public Task RemoveFile(string path)
    {
        return Task.CompletedTask;
    }
}
