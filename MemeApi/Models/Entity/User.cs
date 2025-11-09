#nullable disable warnings
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using MemeApi.Models.Entity.Dubloons;
using MemeApi.Models.Entity.Memes;
using MemeApi.Models.Entity.Places;
using Microsoft.AspNetCore.Identity;

namespace MemeApi.Models.Entity;

public class User : IdentityUser<string>
{
    public string? ProfilePicFile { get; set; }
    public List<Vote> Votes { get; set; }
    public List<Votable> Votables { get; set; }
    public List<Topic>?  Topics { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime LastUpdatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    public List<DubloonEvent> DubloonEvents { get; set; }
    public List<PlaceSubmission> PlaceSubmissions { get; set; }

}

