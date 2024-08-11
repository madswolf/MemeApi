#nullable disable warnings
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemeApi.Models.Entity;

public class Vote
{
    public string Id { get; set; }
    public bool Upvote { get; set; }
    public User User { get; set; }
    public string UserId { get; set; }
    public Votable Element { get; set; }
    public string ElementId { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime LastUpdatedAt { get; set; }
}
