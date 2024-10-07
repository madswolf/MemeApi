#nullable disable warnings
using MemeApi;

namespace MemeApi.Models.Entity.Memes;

public class MemeVisual : Votable
{
    public string Filename { get; set; }
}
