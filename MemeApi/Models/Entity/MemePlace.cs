using System.Collections.Generic;

namespace MemeApi.Models.Entity;

public class MemePlace
{
    public string Id { get; set; }
    public List<PlaceSubmission> PlaceSubmissions { get; set; }

    public int height { get; set; }
    public int width { get; set; }
}
