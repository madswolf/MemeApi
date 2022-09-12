using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MemeApi.library
{
    public interface IFileSaver
    { 
        Task SaveFile(IFormFile file, string path);
    }
}
