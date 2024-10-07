#nullable disable warnings
using MemeApi;

namespace MemeApi.Models.Entity.Memes;

public class MemeText : Votable
{
    public string Text { get; set; }
    public MemeTextPosition Position { get; set; }
}
