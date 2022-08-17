using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace MemeApi.library
{
    public class FileSaver : IFileSaver
    {
        private readonly IConfiguration _configuration;
        public FileSaver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async void SaveFile(IFormFile file, string path)
        {
            await using Stream fileStream = new FileStream(Path.Combine(_configuration["BaseUploadFolder"], path, file.FileName), FileMode.Create);
            await file.CopyToAsync(fileStream);
        }
    }
}
