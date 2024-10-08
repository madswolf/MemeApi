﻿#nullable disable warnings
using MemeApi.Models.Entity.Memes;

namespace MemeApi.Models.Entity;

public class TopicModerator
{
    public string ModeratorId { get; set; }
    public User Moderator { get; set; }

    public string TopicID { get; set; }
    public Topic Topic { get; set; }
}