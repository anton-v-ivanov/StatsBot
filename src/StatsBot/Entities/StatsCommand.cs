using System;
using StatsBot.Managers;

namespace StatsBot.Entities
{
    public class StatsCommand
    {
        private readonly TimeZoneManager _timeZoneManager;

        public DateTimeOffset From { get; private set; }

        public DateTimeOffset To { get; private set; }

        public StatsType Type { get; }

        public string ChatId { get; }

        public StatsCommand(long chatId, StatsType type)
        {
            ChatId = chatId.ToString("D");
            _timeZoneManager = new TimeZoneManager();
            InitDates(DateTimeOffset.UtcNow, type);
            Type = type;
        }

        public StatsCommand(long chatId, DateTime fromDate, DateTime toDate)
        {
            ChatId = chatId.ToString("D");
            From = fromDate;
            To = toDate;
            Type = StatsType.Period;
        }

        private void InitDates(DateTimeOffset now, StatsType type)
        {
            var today = _timeZoneManager.GetMoscowDate(now);
            switch (type)
            {
                case StatsType.Today:
                    From = today;
                    To = today;
                    break;

                case StatsType.Yesterday:
                    From = today.AddDays(-1);
                    To = today.AddDays(-1);
                    break;

                case StatsType.Week:
                    From = today.AddDays(-7);
                    To = today;
                    break;

                case StatsType.Month:
                    From = today.AddMonths(-1);
                    To = today;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public string GetStringTypeModifier()
        {
            switch (Type)
            {
                case StatsType.Today:
                    return "сегодня";

                case StatsType.Yesterday:
                    return "вчера";
                case StatsType.Week:
                    return "за неделю";

                case StatsType.Month:
                    return "за месяц";
                case StatsType.Period:
                    return $"с {From.Date:dd.MM.yy} по {To.Date:dd.MM.yy}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
