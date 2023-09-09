using System.IO;

namespace MemeApi.library;

public class FileRemover : IFileRemover
{
    public void RemoveFile(string path)
    {
        File.Delete(path);
    }
}