using MemeApi.Models.DTO;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemeApi.Models.Entity;

public abstract class DubloonEvent
{
    public string Id { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime EventTimestamp { get; set; }
    public string UserId { get; set; }
    public User Owner {  get; set; }
    public double Dubloons { get; set; }

    public abstract DubloonEventInfoDTO ToDubloonEventInfoDTO();
}
