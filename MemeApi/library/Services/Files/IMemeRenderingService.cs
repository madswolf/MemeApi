using System.Threading.Tasks;
using MemeApi.Models.Entity.Memes;

namespace MemeApi.library.Services.Files;

public interface IMemeRenderingService
{
    public Task<byte[]> RenderMeme(Meme meme);
    public byte[] RenderMemeFromData(byte[] data, string? toptext = null, string? bottomtext = null);
}
