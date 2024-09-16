using MemeApi.Models.DTO;
using System;

namespace MemeApi.Models.Entity;

public class Transaction : DubloonEvent
{
    public User Other {  get; set; }
    public string OtherUserId { get; set; }
    public override DubloonEventInfoDTO ToDubloonEventInfoDTO() => new TransactionInfoDTO(
            Id,
            Owner.UserName,
            (int)Math.Floor(Dubloons),
            OtherUserId
        );
}
