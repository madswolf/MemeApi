using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using MemeApi.library.Authorization;
using MemeApi.Models.Context;
using MemeApi.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemeApi.Controllers;

[ApiController]
[Route("memeoftheday/schedule")]
[Authorize(Policy = Policies.SystemServiceOnly)]
public class MemeOfTheDayController(MemeContext context) : ControllerBase
{
    // The trusted bot enforces Discord Administrator permissions.
    [HttpPut]
    public async Task<IActionResult> SetSchedule(MemeOfTheDayScheduleDTO request, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var schedule = await context.MemeOfTheDaySchedules
            .FromSqlRaw("SELECT * FROM \"MemeOfTheDaySchedules\" WHERE \"Id\" = 1 FOR UPDATE")
            .SingleAsync(cancellationToken);
        schedule.StartMinute = (int)TimeOnly.ParseExact(request.Start, "HH:mm", CultureInfo.InvariantCulture).ToTimeSpan().TotalMinutes;
        schedule.EndMinute = (int)TimeOnly.ParseExact(request.End ?? request.Start, "HH:mm", CultureInfo.InvariantCulture).ToTimeSpan().TotalMinutes;
        schedule.PlanNextRun(DateTime.UtcNow);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Ok(new { request.Start, End = request.End ?? request.Start, TimeZone = "Europe/Copenhagen", schedule.NextRunUtc });
    }
}
