using System;

namespace StatsBot.Managers
{
    public interface ITimeZoneManager
    {
        DateTimeOffset GetMoscowDate(DateTime date);
    }
}