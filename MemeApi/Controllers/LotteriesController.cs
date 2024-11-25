using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MemeApi.library;
using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.library.Repositories;
using MemeApi.Models.DTO.Lotteries;
using MemeApi.Models.DTO.Places;
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
    public async Task<ActionResult<MemePlaceDTO>> CreateLottery([FromBody]LotteryCreationDTO lotteryCreationDTO)
    {
        var lottery = await _lotteryRepository.CreateLottery(lotteryCreationDTO);
        return Ok(lottery);
    }
    
    /// <summary>
    /// Add an item to a lottery
    /// </summary>
    [HttpPost("{bracketId}/items")]
    public async Task<ActionResult<MemePlaceDTO>> AddLotteryItem([FromBody]LotteryItemCreationDTO lotteryItemCreationDto, string bracketId)
    {
        var bracket = await _lotteryRepository.GetLotteryBracket(bracketId);
        if (bracket == null) return NotFound("A bracket with the given id does not exist.");

        var item = await _lotteryRepository.AddLotteryItem(lotteryItemCreationDto, bracket);
        return Ok(item);
    }
    
    /// <summary>
    /// Get a full definition of a lottery.
    /// It includes a list of all items/categories of items, their probability weight and the initial and current stock of each
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<MemePlaceDTO>> GetLotteries()
    {
        var lotteries = await _lotteryRepository.GetLotteries();
        
        return Ok(lotteries.Select(lottery => lottery.ToLotteryDTO(_settings.GetMediaHost())));
    }
    
    /// <summary>
    /// Set status of a lottery
    /// </summary>
    [HttpPost("{lotteryId}/SetStatus")]
    public async Task<ActionResult<MemePlaceDTO>> SetLotteryStatus(string lotteryId, [FromBody]LotteryStatus status)
    {
        var lottery = await _lotteryRepository.SetLotteryStatus(lotteryId, status);
        if (lottery == null) return NotFound("A lottery with the given id does not exist.");

        return Ok(lottery);
    }
    
    //buy a ticket, lotteryId
    /// <summary>
    /// Add an item to a lottery
    /// </summary>
    [HttpPost("{lotteryId}/BuyTicket")]
    public async Task<ActionResult<MemePlaceDTO>> BuyLotteryTicket(string lotteryId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userRepository.GetUser(userId, true);
        
        if(user == null) return NotFound("A user with the given Id does not exist.");
        
        var lottery = await _lotteryRepository.GetLottery(lotteryId);
        if (lottery == null) return NotFound("A lottery with the given id does not exist.");

        if(lottery.TicketCost > user.DubloonEvents.CountDubloons()) 
            BadRequest("Not enough dubloons to make buy a Lottery ticket. Dubloons needed: " + lottery.TicketCost);
        
        var item = await _lotteryRepository.BuyTicket(lottery, user);
        return Ok(item);
    }
    
    //Get a receipt of all items won on a lottery
}