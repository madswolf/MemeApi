using System.Collections.Generic;

namespace MemeApi.Models.Entity.Lottery;

public class LotteryBracket
{
    public string Id { get; set; }
    
    public string LotteryId { get; set; }
    
    public Lottery Lottery { get; set; }

    public string Name { get; set; }
    
    public int ProbabilityWeight { get; set; }
    
    public List<LotteryItem> Items { get; set; }
}