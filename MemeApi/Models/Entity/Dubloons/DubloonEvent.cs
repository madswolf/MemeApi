using MemeApi.Models.DTO.Dubloons;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemeApi.Models.Entity.Dubloons;

public abstract class DubloonEvent
{
    public string Id { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime EventTimestamp { get; set; }
    public string UserId { get; set; }
    public User Owner { get; set; }
    public double Dubloons { get; set; }

    public abstract DubloonEventInfoDTO ToDubloonEventInfoDTO();
}
