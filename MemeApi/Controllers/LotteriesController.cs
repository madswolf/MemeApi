using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MemeApi.library;
using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.library.Repositories;
using MemeApi.Models.DTO.Lotteries;
using MemeApi.Models.Entity.Lottery;
using Microsoft.AspNetCore.Mvc;

namespace MemeApi.Controllers;

/// <summary>
/// A controller for Lotteries.
/// </summary>
[Route("[controller]")]
[ApiController]
public class LotteriesController : ControllerBase
{
    private readonly LotteryRepository _lotteryRepository;
    private readonly UserRepository _userRepository;
    private readonly MemeApiSettings _settings;

    public LotteriesController(LotteryRepository lotteryRepository, MemeApiSettings settings, UserRepository userRepository)
    {
        _lotteryRepository = lotteryRepository;
        _userRepository = userRepository;
        _settings = settings;
    }

    // create a lottery with a name and a list of items with probabilities, urls and such
    /// <summary>
    /// Create a Lottery
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateLottery([FromBody]LotteryCreationDTO lotteryCreationDTO)
    {
        if (Request.Headers["Bot_Secret"] != _settings.GetBotSecret())
            return Unauthorized("You do not have access to this action");
        var lottery = await _lotteryRepository.CreateLottery(lotteryCreationDTO);
        return Ok(lottery);
    }
    
    /// <summary>
    /// Add an item to a lottery
    /// </summary>
    [HttpPost("{bracketId}/items")]
    public async Task<ActionResult> AddLotteryItem([FromForm]LotteryItemCreationDTO lotteryItemCreationDto, string bracketId)
    {
        if (Request.Headers["Bot_Secret"] != _settings.GetBotSecret())
            return Unauthorized("You do not have access to this action");
        var bracket = await _lotteryRepository.GetLotteryBracket(bracketId);
        if (bracket == null) return NotFound("A bracket with the given id does not exist.");
        if (bracket.Lottery.Status != LotteryStatus.Initialized) return Conflict("You cannot add items to a lottery after it has been opened");
        
        var item = await _lotteryRepository.AddLotteryItem(lotteryItemCreationDto, bracket);
        return Ok(item);
    }
    
    /// <summary>
    /// Get a full definition of a lottery.
    /// It includes a list of all items/categories of items, their probability weight and the initial and current stock of each
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<LotteryDTO>> GetLotteries()
    {
        var lotteries = await _lotteryRepository.GetLotteries();
        var easterEggs = _settings.GetAllEasterEggs();
        return Ok(lotteries.Select(lottery =>
        {
            var dto = lottery.ToLotteryDTO(_settings.GetMediaHost());
            var eggs = easterEggs
                .Where(i => i.lotteryId == lottery.Id)
                .Select(i => new LotteryItemDTO
                {
                    //hack to generate the same guid for the easter egg each time
                    ItemId = i.item.ItemThumbnailURL.ExternalUserIdToGuid(),
                    ItemName = i.ItemName,
                    ItemRarityColor = i.item.ItemRarityColor,
                    ItemThumbnail = i.item.ItemThumbnailURL,
                    OutOfStock = false
                })
                .ToList();
            dto.Items.AddRange(eggs);
            return dto;
        }));
    }
    
    /// <summary>
    /// Set status of a lottery
    /// </summary>
    [HttpPost("{lotteryId}/SetStatus")]
    public async Task<ActionResult> SetLotteryStatus(string lotteryId, [FromBody]LotteryStatus status)
    {
        if (Request.Headers["Bot_Secret"] != _settings.GetBotSecret())
            return Unauthorized("You do not have access to this action");
        var lottery = await _lotteryRepository.SetLotteryStatus(lotteryId, status);
        if (lottery == null) return NotFound("A lottery with the given id does not exist.");

        return Ok();
    }
    
    /// <summary>
    /// Draw a ticket from the given Lottery
    /// </summary>
    [HttpPost("{lotteryId}/DrawTicket")]
    public async Task<ActionResult<LotteryTicketDrawDTO>> DrawLotteryTicket(string lotteryId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userRepository.GetUser(userId, true);
        if (Request.Headers["Bot_Secret"] != _settings.GetBotSecret())
            return Unauthorized("You do not have access to this action");        
        
        if(user == null) return NotFound("A user with the given Id does not exist.");
        
        var lottery = await _lotteryRepository.GetLottery(lotteryId);
        if (lottery == null) return NotFound("A lottery with the given id does not exist.");
        if (lottery.Status != LotteryStatus.Open) return Conflict("You cannot currently buy tickets for the lottery with the given Id");

        var result = await _lotteryRepository.DrawTicket(lottery, user);
                
        if(result == null) 
            return BadRequest("Not enough dubloons to draw a Lottery ticket. Dubloons needed: " + lottery.TicketCost);

        return Ok(result);
    }
    
    /// <summary>
    /// Get a receipt of all items won on a lottery
    /// </summary>
    [HttpGet("{lotteryId}/receipt")]
    public async Task<ActionResult<LotteryTicketDrawDTO>> GetReceipt(string lotteryId, [FromQuery] bool verbose = false)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userRepository.GetUser(userId, true);
        
        if(user == null) return NotFound("A user with the given Id does not exist.");
        
        var lottery = await _lotteryRepository.GetLottery(lotteryId);
        if (lottery == null) return NotFound("A lottery with the given id does not exist.");

        var tickets = _lotteryRepository.GetLotteryTickets(user, lottery);
        var items = tickets
            .GroupBy(ticket => ticket.ItemId)
            .Select(g =>
            {
                var ticket = g.OrderByDescending(t => t.EventTimestamp).First();
                var key = verbose ? $"({ticket.ItemId},{ticket.EventTimestamp})" : ticket.Item.Name;
                return $"{key}:  {g.Count()}";
            })
            .ToList();
        
        return Ok(new LotteryReceiptDTO
        {
            LotteryId = lotteryId,
            LotteryTicketPrice = lottery.TicketCost,
            TotalTicketCount = tickets.Count,
            NetDubloonProfit = Convert.ToInt32(Math.Floor(tickets.CountDubloons())),
            Items = items
        });
    }
}