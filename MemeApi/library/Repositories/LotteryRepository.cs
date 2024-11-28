using System;
using System.Collections.Generic;
using System.Linq;
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

        if (lottery != null)
        {
            lottery.Status = status;
        }

        LotteryStatus? newStatus = (CurrentStaus:lottery?.Status, SubmittedStatus:status) switch
        {
            (LotteryStatus.Initialized, LotteryStatus.Open) => LotteryStatus.Open,
            (LotteryStatus.Open, LotteryStatus.Closed) => LotteryStatus.Closed,
            (_, _) => null,
        };

        if (newStatus != null && lottery != null) lottery.Status = (LotteryStatus)newStatus;

        return lottery;
    }

    public async Task<List<string>> DrawTicket(Lottery lottery, User user)
    {
        var random = new Random();
        
        
        
        var bracketItemPairs = lottery.Brackets
            .Select(bracket =>
                {
                    var items = bracket.Items.Where(item => (item.ItemCount - item.Tickets.Count) > 0).ToList();
                    return (Bracket: bracket, Items: items);
                }
            )
            .Where(bracket => bracket.Items.Count != 0)
            .OrderBy(tuple => tuple.Bracket.Id).ToList();
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

        var winningPair = bracketItemPairs[winningBracketIndex];
        var winningItem = winningPair.Items[random.Next(winningPair.Items.Count)];
        bracketItemPairs.RemoveAt(winningBracketIndex);

        var mediaHost = _settings.GetMediaHost();
        var randomItems = 
            bracketItemPairs.Select(tuple => tuple.Items[random.Next(tuple.Items.Count)])
                .ToList()
                .Append(winningItem)
                .Select(item => mediaHost + "lotteryitems/" + item.ThumbNailFileName);
        
        var ticket = new LotteryTicket()
        {
            Id = Guid.NewGuid().ToString(),
            Dubloons = -lottery.TicketCost,
            Item = winningItem,
            Owner = user
        };
        _context.LotteryTickets.Add(ticket);
        await _context.SaveChangesAsync();
        
        return randomItems.ToList();
    }
}