﻿#nullable disable warnings
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemeApi.Models.Entity.Memes;

public abstract class Votable
{
    public string Id { get; set; }
    public string OwnerId { get; set; }
    public User Owner { get; set; }
    public List<Topic> Topics { get; set; }
    public List<Vote> Votes { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime LastUpdatedAt { get; set; }
}
