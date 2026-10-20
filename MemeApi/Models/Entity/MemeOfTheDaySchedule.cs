using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemeApi.Models.Entity;

public class MemeOfTheDaySchedule
{
    public string TimeZoneId { get; set; } = "Europe/Copenhagen";

    [NotMapped]
    public TimeZoneInfo TimeZone => TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);

    public int Id { get; set; } = 1;
    public int StartMinute { get; set; } = 14 * 60 + 30;
    public int EndMinute { get; set; } = 14 * 60 + 30;
    public DateTime? NextRunUtc { get; set; }
    public DateOnly? LastPostedDate { get; set; }

    public void SetTimeZone(string timeZoneId, DateTime utcNow)
    {
        var oldToday = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcNow, TimeZone));
        TimeZoneId = timeZoneId;
        var newToday = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcNow, TimeZone));
        // Preserve whether today's post was sent when switching across calendar dates.
        LastPostedDate = LastPostedDate?.AddDays(newToday.DayNumber - oldToday.DayNumber);
    }

    public void PlanNextRun(DateTime utcNow)
    {
        var timeZone = TimeZone;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone));
        var date = LastPostedDate >= today ? LastPostedDate.Value.AddDays(1) : today;
        while (true)
        {
            var candidates = new List<DateTime>();
            for (var minute = StartMinute; minute <= EndMinute; minute++)
            {
                var localTime = date.ToDateTime(TimeOnly.MinValue).AddMinutes(minute);
                // Skip missing spring-clock minutes; ambiguous autumn minutes use standard time.
                if (timeZone.IsInvalidTime(localTime)) continue;
                var utcTime = TimeZoneInfo.ConvertTimeToUtc(localTime, timeZone);
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
