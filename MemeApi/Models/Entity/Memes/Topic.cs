#nullable disable warnings
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemeApi.Models.Entity.Memes;

public class Topic
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public User Owner { get; set; }
    public string OwnerId { get; set; }
    public List<User> Moderators { get; set; }
    public List<Votable> Votables { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime LastUpdatedAt { get; set; }

    public bool HasRestrictedPosting { get; set; }
}
