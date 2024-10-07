using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MemeApi.library.Services.Files;

public class WebFileLoader : IFileLoader
{
    private readonly MemeApiSettings _settings;

    public WebFileLoader(MemeApiSettings settings)
    {
        _settings = settings;
    }

    public async Task<byte[]> LoadFile(string path)
    {
        using var webClient = new HttpClient();
        var full_url = _settings.GetMediaHost() + path;
        return await webClient.GetByteArrayAsync(new Uri(full_url));
    }
}
