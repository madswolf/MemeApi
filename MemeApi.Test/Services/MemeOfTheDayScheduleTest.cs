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
            var local = TimeZoneInfo.ConvertTimeFromUtc(schedule.NextRunUtc!.Value, schedule.TimeZone);
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

    [Theory]
    [InlineData("America/New_York", "2026-10-24T18:30:00Z")]
    [InlineData("Asia/Kolkata", "2026-10-24T09:00:00Z")]
    [InlineData("UTC", "2026-10-24T14:30:00Z")]
    public void FixedTimeUsesSelectedTimeZone(string timeZone, string expected)
    {
        var schedule = new MemeOfTheDaySchedule { TimeZoneId = timeZone };
        schedule.PlanNextRun(Utc("2026-10-24T06:00:00Z"));
        Assert.Equal(Utc(expected), schedule.NextRunUtc);
        Assert.False(schedule.HasMissedWindow(Utc(expected).AddSeconds(15)));
        Assert.True(schedule.HasMissedWindow(Utc(expected).AddMinutes(1)));
    }

    [Theory]
    [InlineData("2026-10-31T06:00:00Z", "2026-10-31T18:30:00Z")]
    [InlineData("2026-11-01T06:00:00Z", "2026-11-01T19:30:00Z")]
    public void SelectedTimeZoneUsesItsOwnDaylightSavingDates(string now, string expected)
    {
        var schedule = new MemeOfTheDaySchedule { TimeZoneId = "America/New_York" };
        schedule.PlanNextRun(Utc(now));
        Assert.Equal(Utc(expected), schedule.NextRunUtc);
    }

    [Theory]
    [InlineData("America/Los_Angeles", "2026-10-24T00:30:00Z")]
    [InlineData("Pacific/Auckland", "2026-10-24T18:30:00Z")]
    public void ChangingTimeZonePreservesAlreadyPostedToday(string timeZone, string now)
    {
        var schedule = new MemeOfTheDaySchedule { LastPostedDate = new DateOnly(2026, 10, 24) };
        schedule.SetTimeZone(timeZone, Utc(now));
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(Utc(now), schedule.TimeZone));
        Assert.Equal(today, schedule.LastPostedDate);
        schedule.PlanNextRun(Utc(now));
        Assert.Equal(today.AddDays(1), DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(schedule.NextRunUtc!.Value, schedule.TimeZone)));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("Europe/Copenhagen", true)]
    [InlineData("America/New_York", true)]
    [InlineData("UTC", true)]
    [InlineData("", false)]
    [InlineData("MadeUp/TimeZone", false)]
    public void ValidatesTimeZone(string? timeZone, bool valid)
    {
        var dto = new MemeOfTheDayScheduleDTO { Start = "13:00", TimeZone = timeZone };
        Assert.Equal(valid, Validator.TryValidateObject(dto, new ValidationContext(dto), new List<ValidationResult>(), true));
    }

    private static DateTime Utc(string value) => DateTime.Parse(value).ToUniversalTime();
}
