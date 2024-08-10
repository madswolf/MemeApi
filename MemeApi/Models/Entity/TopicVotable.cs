#nullable disable warnings
namespace MemeApi.Models.Entity;

public class TopicVotable
{
    public string VotableId { get; set; }
    public Votable Votable { get; set; }

    public string TopicID { get; set; }
    public Topic Topic { get; set; }

}