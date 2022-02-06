using Microsoft.AspNetCore.Http;

namespace MemeApi.library
{
    public interface IFileSaver
    { 
        void SaveFile(IFormFile file, string path);
    }
}
