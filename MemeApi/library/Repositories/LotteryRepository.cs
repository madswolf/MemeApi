using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MemeApi.library.Extensions;
using MemeApi.library.Services.Files;
using MemeApi.Models.Context;
using MemeApi.Models.DTO.Lotteries;
using MemeApi.Models.Entity;
using MemeApi.Models.Entity.Dubloons;
using MemeApi.Models.Entity.Lottery;
using Microsoft.EntityFrameworkCore;

namespace MemeApi.library.Repositories;

public class LotteryRepository
{
    private readonly MemeContext _context;
    private readonly IFileSaver _fileSaver;
    private readonly MemeApiSettings _settings;
    public const string LotteryitemsPath = "lotteryitems/2025/";

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

        var brackets = lotteryCreationDTO.Brackets.Select((tuple, _) => new LotteryBracket
        {
            Id = Guid.NewGuid().ToString(),
            Name = tuple.BracketName,
            ProbabilityWeight = tuple.ProbabilityWeight,
            RarityColor = tuple.RarityColor,
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
        var itemThumnailFileName = lotteryItemCreationDto.ItemThumbnail.FileName; 
        
        var itemThumbnailExistsWithSameFileName =
            _context.LotteryItems.Any(x => x.ThumbNailFileName == itemThumnailFileName);
        
        if(itemThumbnailExistsWithSameFileName) 
            itemThumnailFileName = itemThumnailFileName.PrependRandomString();
        
        var item = new LotteryItem
        {
            Id = Guid.NewGuid().ToString(),
            Bracket = bracket,
            Name = lotteryItemCreationDto.ItemName,
            ItemCount = lotteryItemCreationDto.ItemCount,
            ThumbNailFileName = itemThumnailFileName,
            ImageFileName = lotteryItemCreationDto.ItemImage != null ? itemThumnailFileName.PrependRandomString() : itemThumnailFileName
        };

        var thumbnail = lotteryItemCreationDto.ItemThumbnail;
        await _fileSaver.SaveFile(thumbnail.ToByteArray(), LotteryitemsPath, item.ThumbNailFileName, thumbnail.ContentType);

        if (lotteryItemCreationDto.ItemImage != null)
        {
            var image = lotteryItemCreationDto.ItemImage;
            await _fileSaver.SaveFile(image.ToByteArray(), LotteryitemsPath, item.ImageFileName, image.ContentType);
        }
        
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
    
    public async Task<LotteryTicketDrawDTO?>
        DrawTicket(Lottery lottery, User user)
    {
        var random = new Random();
        
        var bracketItemPairs = lottery.Brackets
            .Select(bracket =>
                {
                    var items = bracket.Items
                        .Where(item => (item.ItemCount - item.Tickets.Count) > 0 || item.ItemCount == -1)
                        .ToList();
                    return (Bracket: bracket, Items: items);
                }
            )
            .Where(bracket => bracket.Items.Count != 0)
            .OrderByDescending(tuple => tuple.Bracket.ProbabilityWeight).ToList();

        var winningItem = PickItemByBracketWeight(bracketItemPairs, random);
        
        var (price, isFreeSpin) = CalculatePrice(lottery, user, winningItem);
        if (price == null)
        {
            return null;
        }
        
        var ticket = new LotteryTicket
        {
            Id = Guid.NewGuid().ToString(),
            Dubloons = price.Value,
            Item = winningItem,
            Owner = user
        };
        _context.LotteryTickets.Add(ticket);
        await _context.SaveChangesAsync();

        var mediaHost = _settings.GetMediaHost();
        var nonWinningItems = new List<LotteryItemPreviewDTO>();
        
        var oneRandomFromEachBracket = bracketItemPairs.Select(b => b.Items.RandomItem().ToLotteryItemPreviewDTO(mediaHost));
        nonWinningItems.AddRange(oneRandomFromEachBracket);
        
        var includeEasterEgg = random.Next(2) == 0;
        if (includeEasterEgg)
        {
            var easterEgg = _settings.GetEasterEggs(lottery.Id).RandomItem();
            nonWinningItems = nonWinningItems.Append(easterEgg).ToList();
        }

        var targetNonWinningItemCount = 9;
        var extraPaddingNonWinningItems = 
            Enumerable.Range(1, targetNonWinningItemCount - nonWinningItems.Count)
                .Select(_ => PickItemByBracketWeight(bracketItemPairs, random).ToLotteryItemPreviewDTO(mediaHost));
        
        nonWinningItems.AddRange(extraPaddingNonWinningItems);
        
        return new LotteryTicketDrawDTO
        {
            DrawnItemWin =  winningItem.ToLotteryItemWinDTO(mediaHost),
            Items = nonWinningItems,
            WasFree = isFreeSpin
        };
    }

    private (double? price,bool isFreeSpin) CalculatePrice(Lottery lottery, User user, LotteryItem winningItem)
    {
        var dubloons = -lottery.TicketCost;
        
        var tickets = GetLotteryTickets(user, lottery, true);
        var hasUsedAllDailyDiscounts = tickets.Count != 0;

        var isFreeSpin = false;
        if (!hasUsedAllDailyDiscounts)
        {
            isFreeSpin = true;
            dubloons += lottery.TicketCost * 1;
        }
        if (Math.Abs(user.DubloonEvents.CountDubloons()) < Math.Abs(dubloons))
            return (null, false);
        
        var refundPrice = GetRefundAmountByName(winningItem);
        if (refundPrice != null) dubloons += refundPrice.Value;
        return (dubloons, isFreeSpin);
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

    private static int? GetRefundAmountByName(LotteryItem lotteryItem)
    {
        var pattern = @"^(?<Dubloons>[0-9]+) dubloons.*$";
        var match = Regex.Match(lotteryItem.Name.ToLower(), pattern);

        if (!match.Success || !int.TryParse(match.Groups["Dubloons"].Value, out var amount))
        {
            return null;
        }

        return amount;
    }
    
    private static LotteryItem PickItemByBracketWeight(
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

        var (_, items) = bracketItemPairs[winningBracketIndex];
        return items.RandomItem();
    }
}