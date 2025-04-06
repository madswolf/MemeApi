using System.Threading.Tasks;

namespace MemeApi.library.Services.Files;

public interface IFileSaver
{
    Task SaveFile(byte[] file, string path, string fileName, string contentType);
}
