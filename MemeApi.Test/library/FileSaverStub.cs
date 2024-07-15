using MemeApi.library.Services.Files;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MemeApi.Test.library;

internal class FileSaverStub : IFileSaver
{
    public Task SaveFile(IFormFile file, string path, string fileName)
    {
        return Task.CompletedTask;
    }
}
