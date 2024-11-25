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
using MemeApi.Models.Entity.Dubloons;

namespace MemeApi.library.Repositories;

public class LotteryRepository
{
    private readonly MemeContext _context;
    private readonly FileSaver _fileSaver;

    public LotteryRepository(MemeContext context, FileSaver fileSaver)
    {
        _context = context;
        _fileSaver = fileSaver;
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

        _context.Lotteries.Add(lottery);
        await _context.SaveChangesAsync();
        
        return lottery;
    }

    public async Task<Lottery?> GetLottery(string lotteryId)
    {
        return await _context.Lotteries.FirstOrDefaultAsync(lottery => lottery.Id == lotteryId);
    }
    
    public async Task<List<Lottery>> GetLotteries()
    {
        return await _context.Lotteries
            .Include(lottery => lottery.Items)
            .ThenInclude(item => item.Tickets)
            .ToListAsync();
    }
    
    public async Task<LotteryItem> AddLotteryItem(LotteryItemCreationDTO lotteryItemCreationDto, Lottery lottery)
    {
        var item = new LotteryItem()
        {
            Id = Guid.NewGuid().ToString(),
            Lottery = lottery,
            Name = lotteryItemCreationDto.ItemName,
            ProbabilityWeight = lotteryItemCreationDto.ItemProbabilityWeight,
            ItemCount = lotteryItemCreationDto.ItemCount,
            ThumbNailFileName = lotteryItemCreationDto.ItemThumbnail.FileName
        };

        var itemExistsWithSameFileName =
            _context.LotteryItems.Any(x => x.ThumbNailFileName == item.ThumbNailFileName);
        
        if(itemExistsWithSameFileName) 
            item.ThumbNailFileName = item.ThumbNailFileName.PrependRandomString();

        var thumbnail = lotteryItemCreationDto.ItemThumbnail;
        await _fileSaver.SaveFile(thumbnail.ToByteArray(), "lotteryitems/", item.ThumbNailFileName, thumbnail.ContentType);
        
        _context.Lotteries.Add(lottery);
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

    public async Task<LotteryTicket> BuyTicket(Lottery lottery, string userId)
    {
        var item = 
    }
}