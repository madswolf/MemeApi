#nullable disable warnings
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using MemeApi.Models.DTO.Memes;

namespace MemeApi.Models.Entity.Memes;

public abstract class Votable
{
    public string Id { get; set; }
    public string OwnerId { get; set; }
    public string ContentHash { get; set; }
    public User Owner { get; set; }
    public List<Topic> Topics { get; set; }
    public List<Vote> Votes { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime LastUpdatedAt { get; set; }

    public abstract VotableComponentDTO ToComponentDTO(string mediaHost);

    public double VoteAverage()
    {
        var voteSum = Votes.Sum(v => v.VoteNumber);
        var totalVotes = Votes.Count;
        if (voteSum == 0 || totalVotes == 0) return 0.0;
        var average = voteSum/(totalVotes * 1.0);
        
        return average + (totalVotes * 1e-6);
    }
}
