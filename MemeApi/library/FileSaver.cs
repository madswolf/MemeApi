using System.IO;
using System.Threading.Tasks;
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

        public async Task SaveFile(IFormFile file, string path)
        {
            var completePath = Path.Combine(_configuration["BaseUploadFolder"], path, file.FileName);
            using (Stream fileStream = new FileStream(completePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            };
        }
    }
}
