﻿using System;
using MemeApi.Models.DTO.Dubloons;

namespace MemeApi.Models.Entity.Dubloons;
using Lottery;

public class LotteryTicket : DubloonEvent
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