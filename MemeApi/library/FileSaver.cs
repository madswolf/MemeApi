using System.IO;
using Microsoft.AspNetCore.Http;

namespace MemeApi.library
{
    public class FileSaver : IFileSaver
    {
        public async void SaveFile(IFormFile file, string path)
        {
            await using Stream fileStream = new FileStream(Path.Combine(path, file.FileName), FileMode.Create);
            await file.CopyToAsync(fileStream);
        }
    }
}
