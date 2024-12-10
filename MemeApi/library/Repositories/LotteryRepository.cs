using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MemeApi.Models.Context;
using MemeApi.Models.DTO.Lotteries;
using MemeApi.Models.Entity.Lottery;
using Microsoft.EntityFrameworkCore;
using MemeApi.library.Extensions;
using MemeApi.library.Services.Files;
using MemeApi.Models.Entity;
using MemeApi.Models.Entity.Dubloons;

namespace MemeApi.library.Repositories;

public class LotteryRepository
{
    private readonly MemeContext _context;
    private readonly IFileSaver _fileSaver;
    private readonly MemeApiSettings _settings;

    public LotteryRepository(MemeContext context, IFileSaver fileSaver, MemeApiSettings settings)
    {
        _context = context;
        _fileSaver = fileSaver;
        _settings = settings;
    }

    public async Task<Lottery> CreateLottery(LotteryCreationDTO lotteryCreationDTO)
    {
        var lottery = new Lottery
        {
            Id = Guid.NewGuid().ToString(),
            Name = lotteryCreationDTO.LotteryName,
            Status = LotteryStatus.Initialized,
            TicketCost = lotteryCreationDTO.TicketCost
        };

        var brackets = lotteryCreationDTO.Brackets.Select((tuple, _) => new LotteryBracket()
        {
            Id = Guid.NewGuid().ToString(),
            Name = tuple.BracketName,
            ProbabilityWeight = tuple.ProbabilityWeight,
            Lottery = lottery
        });
        
        _context.LotteryBrackets.AddRange(brackets);
        _context.Lotteries.Add(lottery);
        await _context.SaveChangesAsync();
        
        return lottery;
    }

    public async Task<Lottery?> GetLottery(string lotteryId)
    {
        return await _context.Lotteries
            .Include(lottery => lottery.Brackets)
            .ThenInclude(bracket => bracket.Items)
            .ThenInclude(item => item.Tickets)
            .FirstOrDefaultAsync(lottery => lottery.Id == lotteryId);
    }
    
    public async Task<LotteryBracket?> GetLotteryBracket(string bracketId)
    {
        return await _context.LotteryBrackets
            .Include(bracket => bracket.Lottery)
            .FirstOrDefaultAsync(lottery => lottery.Id == bracketId);
    }
    
    public async Task<List<Lottery>> GetLotteries()
    {
        return await _context.Lotteries
            .Include(lottery => lottery.Brackets)
            .ThenInclude(bracket => bracket.Items)
            .ThenInclude(item => item.Tickets)
            .ToListAsync();
    }
    
    public async Task<LotteryItem> AddLotteryItem(LotteryItemCreationDTO lotteryItemCreationDto, LotteryBracket bracket)
    {
        var item = new LotteryItem()
        {
            Id = Guid.NewGuid().ToString(),
            Bracket = bracket,
            Name = lotteryItemCreationDto.ItemName,
            ItemCount = lotteryItemCreationDto.ItemCount,
            ThumbNailFileName = lotteryItemCreationDto.ItemThumbnail.FileName
        };

        var itemExistsWithSameFileName =
            _context.LotteryItems.Any(x => x.ThumbNailFileName == item.ThumbNailFileName);
        
        if(itemExistsWithSameFileName) 
            item.ThumbNailFileName = item.ThumbNailFileName.PrependRandomString();

        var thumbnail = lotteryItemCreationDto.ItemThumbnail;
        await _fileSaver.SaveFile(thumbnail.ToByteArray(), "lotteryitems/", item.ThumbNailFileName, thumbnail.ContentType);
        
        _context.LotteryItems.Add(item);
        await _context.SaveChangesAsync();
        
        return item;
    }
    
    public async Task<Lottery?> SetLotteryStatus(string lotteryId, LotteryStatus status)
    {
        var lottery = await _context.Lotteries.FirstOrDefaultAsync(lottery => lottery.Id == lotteryId);
        
        LotteryStatus? newStatus = (CurrentStaus:lottery?.Status, SubmittedStatus:status) switch
        {
            (LotteryStatus.Initialized, LotteryStatus.Open) => LotteryStatus.Open,
            (LotteryStatus.Open, LotteryStatus.Closed) => LotteryStatus.Closed,
            (LotteryStatus.Open, LotteryStatus.Initialized) => LotteryStatus.Initialized,
            (_, _) => null,
        };

        if (newStatus != null && lottery != null)
        {
            lottery.Status = (LotteryStatus)newStatus;
            await _context.SaveChangesAsync();
        }

        return lottery;
    }

    public async Task<(List<string>? items, (string winningItem, string winningItemName, int winRarity, bool wasFree))>
        DrawTicket(Lottery lottery, User user)
    {
        var random = new Random();
        
        var bracketItemPairs = lottery.Brackets
            .Select(bracket =>
                {
                    var items = bracket.Items.Where(item => (item.ItemCount - item.Tickets.Count) > 0 || item.ItemCount == -1).ToList();
                    return (Bracket: bracket, Items: items);
                }
            )
            .Where(bracket => bracket.Items.Count != 0)
            .OrderByDescending(tuple => tuple.Bracket.ProbabilityWeight).ToList();

        var (winningBracketIndex, winRarity) = WeightedRandomIndex(bracketItemPairs, random);

        var winningPair = bracketItemPairs[winningBracketIndex];
        var winningItem = winningPair.Items[random.Next(winningPair.Items.Count)];

        var dubloons = -lottery.TicketCost;
        
        var tickets = GetLotteryTickets(user, lottery, true);
        var hasUsedAllDailyDiscounts = tickets.Count != 0;

        var isFreeSpin = false;
        if (!hasUsedAllDailyDiscounts)
        {
            isFreeSpin = true;
            dubloons += (int)(lottery.TicketCost * 1);
        }
        if (Math.Abs(user.DubloonEvents.CountDubloons()) < Math.Abs(dubloons))
            return (null, default);
        
        var refundPrice = GetRefundPercentageByName(winningItem);
        if (refundPrice != null) dubloons += (int)(lottery.TicketCost * ((int)refundPrice/100.0));
        
        var ticket = new LotteryTicket()
        {
            Id = Guid.NewGuid().ToString(),
            Dubloons = dubloons,
            Item = winningItem,
            Owner = user
        };
        _context.LotteryTickets.Add(ticket);
        await _context.SaveChangesAsync();

        if (bracketItemPairs.Count > 1)
        {
            bracketItemPairs.RemoveAt(winningBracketIndex);
        }
        
        var mediaHost = _settings.GetMediaHost();
        var randomItems =
            bracketItemPairs
                .Select(tuple => tuple.Items[random.Next(tuple.Items.Count)].ToThumbnailUrl(mediaHost))
                .ToList();

        var extraItemCount = 9 - randomItems.Count;
        var extraItems = new List<string>(new String[extraItemCount]);
        
        var includeEasterEgg = random.Next(2) == 0;
        if (includeEasterEgg)
        {
            var easterEgg = _settings.GetEasterEggs();
            extraItems[extraItemCount - 1] = easterEgg[random.Next(easterEgg.Count)];
            extraItemCount -= 1;
        }
        
        for (var i = 0; i < extraItemCount; i++)
        {
            var (randomBracketIndex, _) = WeightedRandomIndex(bracketItemPairs, random);
            var pair = bracketItemPairs[randomBracketIndex];
            extraItems[i] = pair.Items[random.Next(pair.Items.Count)].ToThumbnailUrl(mediaHost);
        }
        

        randomItems.AddRange(extraItems);
        
        return (randomItems, (winningItem.ToThumbnailUrl(mediaHost), winningItem.Name, winRarity, isFreeSpin));
    }
    
    
    public  List<LotteryTicket> GetLotteryTickets(User user, Lottery lottery, bool filter = false)
    {
        var offset = new TimeSpan(6, 0, 0);
        var now = (DateTime.UtcNow + offset).Date;
        
        return _context.LotteryTickets
            .Include(ticket => ticket.Item)
            .ThenInclude(item => item.Bracket)
            .Where(ticket => ticket.Owner == user && ticket.Item.Bracket.LotteryId == lottery.Id && (!filter || (ticket.EventTimestamp + offset).Date == now))
            .ToList();
    }

    private static int? GetRefundPercentageByName(LotteryItem lotteryItem)
    {
        var pattern = @"^(\d+)\s*(.*)$";
        var match = Regex.Match(lotteryItem.Name, pattern);

        if (!match.Success || !int.TryParse(match.Groups[1].Value, out var percentage))
        {
            return null;
        }

        return percentage;
    }
    
    private static (int WinningIndex, int LikelihoodPercentage) WeightedRandomIndex(
        List<(LotteryBracket Bracket, List<LotteryItem> Items)> bracketItemPairs, 
        Random random)
    {
        var totalWeight = bracketItemPairs.Sum(tuple => tuple.Bracket.ProbabilityWeight);
        var randomValue = random.Next(totalWeight);
        var cumulativeWeight = 0;
        var winningBracketIndex = 0;

        for (var i = 0; i < bracketItemPairs.Count; i++)
        {
            cumulativeWeight += bracketItemPairs[i].Bracket.ProbabilityWeight;
            if (randomValue < cumulativeWeight)
            {
                winningBracketIndex = i;
                break;
            }
        }

        var winningBracketWeight = bracketItemPairs[winningBracketIndex].Bracket.ProbabilityWeight;
        
        var lowerWeightSum = bracketItemPairs
            .Where(tuple => tuple.Bracket.ProbabilityWeight > winningBracketWeight)
            .Sum(tuple => tuple.Bracket.ProbabilityWeight);

        var lowerWeightLikelihoodPercentage = (int)Math.Round((double)lowerWeightSum / totalWeight * 100);

        return (winningBracketIndex, lowerWeightLikelihoodPercentage);
    }
}