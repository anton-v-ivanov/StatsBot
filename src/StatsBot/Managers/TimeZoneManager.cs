using System;
using NodaTime;
using Serilog;

namespace TlenBot.Managers
{
    public class TimeZoneManager: ITimeZoneManager
    {
        private static DateTimeZone MoscowTimeZone => DateTimeZoneProviders.Tzdb["Europe/Moscow"];
        //private static DateTimeZone MoscowTimeZone => DateTimeZoneProviders.Tzdb["America/New_York"];

        public DateTime GetMoscowNowDate()
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            return now.InZone(MoscowTimeZone).Date.ToDateTimeUnspecified();
        }

        public DateTime GetMoscowNowTime()
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            return now.InZone(MoscowTimeZone).ToDateTimeUnspecified();
        }

        public Tuple<int, int> GetMoscowEndOfDayInLocalTime()
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var localTz = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            var localZonedTime = now.InZone(localTz);
            var moscowZonedTime = now.InZone(MoscowTimeZone);
            var localTime = new LocalTime(localZonedTime.Hour, localZonedTime.Minute);
            var moscowTime = new LocalTime(moscowZonedTime.Hour, moscowZonedTime.Minute);
            var diff = localTime.Minus(moscowTime);
            var result = new LocalTime(0, 0).Plus(diff);
            Log.Information("Local time: {localTime}, Moscow time: {moscowTime}, TimeZone diff: {diff}", localTime, moscowTime, diff);
            return new Tuple<int, int>(result.Hour, result.Minute);
        }
    }
}