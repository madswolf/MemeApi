using System.Collections.Generic;
using MemeApi.Models.Entity.Dubloons;

namespace MemeApi.Models.Entity.Lottery;

public class LotteryItem
{
    public string Id { get; set; }
    
    public string BracketId { get; set; }
    
    public LotteryBracket Bracket { get; set; }

    public string Name { get; set; }
    
    public int ItemCount { get; set; }
    
    public string ThumbNailFileName { get; set; }
    
    public string ImageFileName { get; set; }
    
    public List<LotteryTicket> Tickets { get; set; }
}