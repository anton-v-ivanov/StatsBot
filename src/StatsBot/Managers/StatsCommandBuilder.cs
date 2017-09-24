using System;
using System.Globalization;
using TlenBot.Entities;

namespace TlenBot.Managers
{
    public class StatsCommandBuilder: IStatsCommandBuilder
    {
        private readonly ITimeZoneManager _timeZoneManager;

        public StatsCommandBuilder(ITimeZoneManager timeZoneManager)
        {
            _timeZoneManager = timeZoneManager;
        }

        public StatsCommand BuildFromString(string text)
        {
            // /stats 11.02.2017 12.03.2017 @tonageme -165034900
            // /stats
            // /stats сегодня
            // /stats вчера
            // /stats неделя
            // /stats месяц
            var tokens = text.Split(' ');
            var now = _timeZoneManager.GetMoscowNowDate();

            if (tokens.Length == 2)
            {
                var modifier = tokens[1];
                if (modifier.Equals("сегодня", StringComparison.OrdinalIgnoreCase))
                    return new StatsCommand(now, now, StatsType.Today);

                if (modifier.Equals("вчера", StringComparison.OrdinalIgnoreCase))
                    return new StatsCommand(now.AddDays(-1), now.AddDays(-1), StatsType.Yesterday);

                if (modifier.Equals("неделя", StringComparison.OrdinalIgnoreCase))
                    return new StatsCommand(now.AddDays(-7), now, StatsType.Week);

                if (modifier.Equals("месяц", StringComparison.OrdinalIgnoreCase))
                    return new StatsCommand(now.AddDays(-30), now, StatsType.Month);
            }

            if (tokens.Length > 2)
            {
                // /stats 11.02.2017 12.03.2017 @tonageme -165034900
                if (!DateTime.TryParseExact(tokens[1], "dd.MM.yy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime from))
                {
                    throw new FormatException("Формат для даты: dd.mm.yy. Пример: 29.01.17");
                }

                if (!DateTime.TryParseExact(tokens[2], "dd.MM.yy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime to))
                {
                    throw new FormatException("Формат для даты: dd.mm.yy. Пример: 29.01.17");
                }

                var command = new StatsCommand(from, to, StatsType.Period);
                if (tokens.Length > 3)
                {
                    if (int.TryParse(tokens[3], out int id))
                        command.UserId = id;
                    else
                        command.UserName = tokens[3].Replace("@", "");
                }
                if (tokens.Length > 4 && Consts.IsDev)
                {
                    command.ChatId = int.Parse(tokens[4]);
                }
                return command;
            }

            return new StatsCommand(now, now, StatsType.Today);
        }
    }
}