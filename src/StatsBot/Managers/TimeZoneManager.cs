using System;
using NodaTime;
using NodaTime.Extensions;

namespace StatsBot.Managers
{
    public class TimeZoneManager : ITimeZoneManager
    {
        private static DateTimeZone MoscowTimeZone => DateTimeZoneProviders.Tzdb["Europe/Moscow"];

        public DateTimeOffset GetMoscowDate(DateTime date) =>
            date.ToInstant().InZone(MoscowTimeZone).ToDateTimeOffset();

        public DateTimeOffset GetMoscowDate(DateTimeOffset date) =>
            date.ToInstant().InZone(MoscowTimeZone).ToDateTimeOffset();
    }
}
