using System;
using System.Runtime.InteropServices;

namespace StatsBot.Managers
{
    public class TimeZoneManager
    {
        private static TimeZoneInfo MoscowTimeZone
        {
            get
            {
                var tzName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "Russian Standard Time"
                    : "Europe/Moscow";
                return TimeZoneInfo.FindSystemTimeZoneById(tzName);
            }
        }

        public static DateTime GetMoscowNowDate()
        {
            return TimeZoneInfo.ConvertTime(DateTime.Now, MoscowTimeZone).AddHours(-1).Date;
        }

        public static DateTime GetMoscowNowTime()
        {
            return TimeZoneInfo.ConvertTime(DateTime.Now, MoscowTimeZone).AddHours(-1);
        }
    }
}