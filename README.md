# MemeApi

MemeApi is an API for creating, storing and generating memes.

MemeApi [![.NET](https://github.com/MadsWJespersen/MemeApi/actions/workflows/dotnet.yml/badge.svg)](https://github.com/MadsWJespersen/MemeApi/actions/workflows/dotnet.yml)

## Installation (locally)

Install postgresql and create a database and adjust the launchSettings.json so that the environment variable DATABASE_CONNECTION_STRING is set.

## Dagens hjerneskade scheduling

The existing daily-post service and webhook are controlled by a single database
schedule. The default is 14:30 in `Europe/Copenhagen`, with daylight saving handled
automatically. MonsterBot administrators can update it through
`PUT /memeoftheday/schedule` using the existing `Bot_Secret` authentication:

```json
{ "start": "13:00", "end": "15:00" }
```

Omit `end` for a fixed time. Times are 24-hour `HH:mm`; intervals stay within one
day. A random minute is saved for each day, so restarting does not reroll it.
Changes apply to today's remaining interval unless today's post was already sent.
Missed intervals are skipped; the scheduler polls every 15 seconds. Missing
spring-clock minutes are skipped and repeated autumn minutes use standard time.

The migration runs through the existing startup migration setup. Deploy this API
change before the MonsterBot slash command. No webhook configuration changes are needed.

## Installation (produktion)
