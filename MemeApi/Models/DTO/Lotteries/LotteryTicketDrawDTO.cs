using System.Collections.Generic;

namespace MemeApi.Models.DTO.Lotteries;

public record LotteryTicketDrawDTO
{
        public List<string> Items { get; init; }
        public string WinningItem { get; init; }
}