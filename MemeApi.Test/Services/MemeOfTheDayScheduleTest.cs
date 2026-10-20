using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Xunit;

namespace MemeApi.Test.Services;

public class MemeOfTheDayScheduleTest
{
    [Theory]
    [InlineData("2026-10-24T00:00:00Z", "2026-10-24T12:30:00Z")]
    [InlineData("2026-10-25T00:00:00Z", "2026-10-25T13:30:00Z")]
    [InlineData("2026-03-28T00:00:00Z", "2026-03-28T13:30:00Z")]
    [InlineData("2026-03-29T00:00:00Z", "2026-03-29T12:30:00Z")]
    public void FixedTimeStaysAt1430InCopenhagenAcrossClockChanges(string now, string expected)
    {
        var schedule = new MemeOfTheDaySchedule();
        schedule.PlanNextRun(Utc(now));
        Assert.Equal(Utc(expected), schedule.NextRunUtc);
    }

    [Theory]
    [InlineData("2026-10-24T00:00:00Z")]
    [InlineData("2026-10-25T00:00:00Z")]
    public void RandomTimeIsInsideLocalInterval(string now)
    {
        var schedule = new MemeOfTheDaySchedule { StartMinute = 13 * 60, EndMinute = 15 * 60 };
        for (var i = 0; i < 50; i++)
        {
            schedule.PlanNextRun(Utc(now));
            var local = TimeZoneInfo.ConvertTimeFromUtc(schedule.NextRunUtc!.Value, MemeOfTheDaySchedule.TimeZone);
            Assert.InRange(local.TimeOfDay, TimeSpan.FromHours(13), TimeSpan.FromHours(15));
            Assert.Equal(Utc(now).Date, local.Date);
        }
    }

    [Fact]
    public void ChangingTimeAfterPostingSchedulesTomorrow()
    {
        var schedule = new MemeOfTheDaySchedule { LastPostedDate = new DateOnly(2026, 10, 24) };
        schedule.PlanNextRun(Utc("2026-10-24T10:00:00Z"));
        Assert.Equal(Utc("2026-10-25T13:30:00Z"), schedule.NextRunUtc);
    }

    [Fact]
    public void ExpiredWindowIsSkipped()
    {
        var schedule = new MemeOfTheDaySchedule { NextRunUtc = Utc("2026-10-24T12:30:00Z") };
        Assert.False(schedule.HasMissedWindow(Utc("2026-10-24T12:30:15Z")));
        Assert.True(schedule.HasMissedWindow(Utc("2026-10-24T12:31:00Z")));
        schedule.PlanNextRun(Utc("2026-10-24T12:31:00Z"));
        Assert.Equal(Utc("2026-10-25T13:30:00Z"), schedule.NextRunUtc);
    }

    [Theory]
    [InlineData("2026-03-29T00:00:00Z", "2026-03-30T00:30:00Z")]
    [InlineData("2026-10-25T00:00:00Z", "2026-10-25T01:30:00Z")]
    public void MissingAndRepeatedClockTimesAreHandledOnce(string now, string expected)
    {
        var schedule = new MemeOfTheDaySchedule { StartMinute = 150, EndMinute = 150 };
        schedule.PlanNextRun(Utc(now));
        Assert.Equal(Utc(expected), schedule.NextRunUtc);
    }

    [Fact]
    public void IntervalEndingInMissingClockHourDoesNotThrow()
    {
        var schedule = new MemeOfTheDaySchedule
        {
            StartMinute = 60, EndMinute = 150, NextRunUtc = Utc("2026-03-29T00:30:00Z")
        };
        Assert.False(schedule.HasMissedWindow(Utc("2026-03-29T00:30:15Z")));
        Assert.True(schedule.HasMissedWindow(Utc("2026-03-29T01:01:00Z")));
    }

    [Theory]
    [InlineData("13:00", null, true)]
    [InlineData("13:00", "15:00", true)]
    [InlineData("13:00", "13:00", true)]
    [InlineData("24:00", null, false)]
    [InlineData("13:60", null, false)]
    [InlineData("1pm", null, false)]
    [InlineData("13:00", "12:59", false)]
    [InlineData("13:00", "", false)]
    public void ValidatesTimeInput(string start, string? end, bool valid)
    {
        var dto = new MemeOfTheDayScheduleDTO { Start = start, End = end };
        Assert.Equal(valid, Validator.TryValidateObject(dto, new ValidationContext(dto), new List<ValidationResult>(), true));
    }

    private static DateTime Utc(string value) => DateTime.Parse(value).ToUniversalTime();
}
