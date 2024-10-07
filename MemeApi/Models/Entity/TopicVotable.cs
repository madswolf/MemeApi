#nullable disable warnings
using MemeApi.Models.Entity.Memes;

namespace MemeApi.Models.Entity;

public class TopicVotable
{
    public string VotableId { get; set; }
    public Votable Votable { get; set; }

    public string TopicID { get; set; }
    public Topic Topic { get; set; }

}