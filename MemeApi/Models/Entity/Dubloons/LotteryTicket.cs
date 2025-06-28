using System;
using MemeApi.Models.DTO.Dubloons;
using MemeApi.Models.Entity.Lottery;

namespace MemeApi.Models.Entity.Dubloons;

public record LotteryTicket : DubloonEvent
{
    public string ItemId { get; set; }
    
    public LotteryItem Item { get; set; }
    public override DubloonEventInfoDTO ToDubloonEventInfoDTO() => new LotteryTicketDTO(
        Id,
        Owner.UserName,
        (int)Math.Floor(Dubloons),
        ItemId
    );
}