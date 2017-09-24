using System;

namespace TlenBot.Managers
{
    public interface ITimeZoneManager
    {
        DateTime GetMoscowNowDate();
        DateTime GetMoscowNowTime();
        Tuple<int, int> GetMoscowEndOfDayInLocalTime();
    }
}