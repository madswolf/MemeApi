﻿using System.Collections.Generic;

namespace MemeApi.Models.DTO.Lotteries;

public record LotteryReceiptDTO
{
    public string LotteryId { get; init; }
    
    public int LotteryTicketPrice { get; init; }
    public int TotalTicketCount { get; init; }
    public int NetDubloonProfit { get; init; }
    public List<string> Items { get; init; }
}

public record LotteryItemReceiptDTO
{
    public string ItemId { get; init; }
    public string ItemName { get; init; }
    public int ItemCount { get; init; }
}