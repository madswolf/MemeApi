using MemeApi.Models.Entity.Memes;
using System.Threading.Tasks;

namespace MemeApi.library.Services.Files;

public interface IMemeRenderingService
{
    public Task<byte[]> RenderMeme(Meme meme);
    public byte[] RenderMemeFromData(byte[] data, string? toptext = null, string? bottomtext = null);
}
