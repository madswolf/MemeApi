using System.Collections.Generic;

namespace MemeApi.Models.DTO.Lotteries;

public record LotteryReceiptDTO
{
    public string LotteryId { get; init; }
    public int TotalTicketCount { get; init; }
    public int TotalDubloonsSpent { get; init; }
    public List<LotteryItemReceiptDTO> Items { get; init; }
}

public record LotteryItemReceiptDTO
{
    public string ItemId { get; init; }
    public string ItemName { get; init; }
    public int ItemCount { get; init; }
}