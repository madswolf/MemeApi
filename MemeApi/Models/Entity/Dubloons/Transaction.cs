using System;
using MemeApi.Models.DTO.Dubloons;

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
