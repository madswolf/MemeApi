using System;
using System.Collections.Generic;

namespace MemeApi.Models.Entity;

public class MemeOfTheDaySchedule
{
    public static readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen");

    public int Id { get; set; } = 1;
    public int StartMinute { get; set; } = 14 * 60 + 30;
    public int EndMinute { get; set; } = 14 * 60 + 30;
    public DateTime? NextRunUtc { get; set; }
    public DateOnly? LastPostedDate { get; set; }

    public void PlanNextRun(DateTime utcNow)
    {
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcNow, TimeZone));
        var date = LastPostedDate >= today ? LastPostedDate.Value.AddDays(1) : today;
        while (true)
        {
            var candidates = new List<DateTime>();
            for (var minute = StartMinute; minute <= EndMinute; minute++)
            {
                var localTime = date.ToDateTime(TimeOnly.MinValue).AddMinutes(minute);
                // Skip missing spring-clock minutes; ambiguous autumn minutes use standard time.
                if (TimeZone.IsInvalidTime(localTime)) continue;
                var utcTime = TimeZoneInfo.ConvertTimeToUtc(localTime, TimeZone);
                if (utcTime >= utcNow) candidates.Add(utcTime);
            }

            if (candidates.Count > 0)
            {
                NextRunUtc = candidates[Random.Shared.Next(candidates.Count)];
                return;
            }
            date = date.AddDays(1);
        }
    }

    public bool HasMissedWindow(DateTime utcNow)
    {
        if (NextRunUtc == null) return true;
        var scheduledDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(NextRunUtc.Value, TimeZone));
        var localEnd = scheduledDate.ToDateTime(TimeOnly.MinValue).AddMinutes(EndMinute);
        while (TimeZone.IsInvalidTime(localEnd)) localEnd = localEnd.AddMinutes(1);
        // Allow the polling loop to deliver during the final scheduled minute.
        return utcNow >= TimeZoneInfo.ConvertTimeToUtc(localEnd, TimeZone).AddMinutes(1);
    }
}
