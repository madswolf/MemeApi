using MemeApi.Models.DTO;
using MemeApi.Models.DTO.Dubloons;
using System;

namespace MemeApi.Models.Entity.Dubloons;

public class Transaction : DubloonEvent
{
    public User Other { get; set; }
    public string OtherUserId { get; set; }
    public override DubloonEventInfoDTO ToDubloonEventInfoDTO() => new TransactionInfoDTO(
            Id,
            Owner.UserName,
            (int)Math.Floor(Dubloons),
            OtherUserId
        );
}
