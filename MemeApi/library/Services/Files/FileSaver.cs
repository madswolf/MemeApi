﻿using System.IO;
using System.Threading.Tasks;

namespace MemeApi.library.Services.Files;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class FileSaver : IFileSaver
{
    private readonly MemeApiSettings _settings;
    public FileSaver(MemeApiSettings settings)
    {
        _settings = settings;
    }

    public async Task SaveFile(byte[] file, string path, string fileName, string contentType)
    {
        var completePath = Path.Combine(_settings.GetBaseUploadFolder(), path, fileName);
        using (Stream fileStream = new FileStream(completePath, FileMode.Create))
        {
            await fileStream.WriteAsync(file);
        };
    }
}
