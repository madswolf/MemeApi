using System.IO;
using Microsoft.AspNetCore.Http;

namespace MemeApi.library
{
    public class FileRemover : IFileRemover
    {
        public void RemoveFile(string path)
        {
            File.Delete(path);
        }
    }
}