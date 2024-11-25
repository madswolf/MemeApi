using System.Collections.Generic;

namespace MemeApi.Models.Entity.Lottery;

public class Lottery
{
    public string Id { get; set; }
    
    public string Name { get; set; }
    
    public int TicketCost { get; set; }
    
    public LotteryStatus Status { get; set; }
    
    public List<LotteryItem> Items { get; set; }
}

public enum LotteryStatus
{
    Initialized,
    Open,
    Closed
}