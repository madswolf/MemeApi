#nullable disable warnings
namespace MemeApi.Models.Entity;

public class MemeText
{
    public string Id { get; set; }
    public string VotableId { get; set; }
    public Votable Votable { get; set; }
    public string Text { get; set; }
    public MemeTextPosition Position { get; set; }
}
