#nullable disable warnings
namespace MemeApi.Models.Entity;

public class MemeVisual
{
    public string VotableId { get; set; }
    public Votable Votable { get; set; }
    public string Filename { get; set; }
}
