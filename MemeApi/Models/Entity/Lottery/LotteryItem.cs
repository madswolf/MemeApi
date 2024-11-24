using System.Collections.Generic;
using MemeApi.Models.Entity.Dubloons;

namespace MemeApi.Models.Entity.Lottery;

public class LotteryItem
{
    public string Id { get; set; }
    
    public string LotteryId { get; set; }
    
    public Lottery Lottery { get; set; }

    public string Name { get; set; }
    
    public string ThumbNailFileName { get; set; }
    
    public List<LotteryTicket> Tickets { get; set; }
}