using System;
using System.Globalization;
using StatsBot.Entities;

namespace StatsBot.Commands
{
    public class StatsCommandBuilder : IStatsCommandBuilder
    {
        public StatsCommand BuildFromString(long chatId, string text)
        {
            // /stats 11.02.2017 12.03.2017
            // /stats
            // /stats сегодня
            // /stats вчера
            // /stats неделя
            // /stats месяц
            var tokens = text.Split(' ');

            if (tokens.Length == 2)
            {
                var modifier = tokens[1];
                if (modifier.Equals("сегодня", StringComparison.OrdinalIgnoreCase))
                    return new StatsCommand(chatId, StatsType.Today);

                if (modifier.Equals("вчера", StringComparison.OrdinalIgnoreCase))
                    return new StatsCommand(chatId, StatsType.Yesterday);

                if (modifier.Equals("неделя", StringComparison.OrdinalIgnoreCase))
                    return new StatsCommand(chatId, StatsType.Week);

                if (modifier.Equals("месяц", StringComparison.OrdinalIgnoreCase))
                    return new StatsCommand(chatId, StatsType.Month);
            }

            if (tokens.Length > 2)
            {
                // /stats 11.02.2017 12.03.2017
                if (!DateTime.TryParseExact(tokens[1], "dd.MM.yy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var from))
                {
                    throw new FormatException("Формат для даты: dd.mm.yy. Пример: 29.01.17");
                }

                if (!DateTime.TryParseExact(tokens[2], "dd.MM.yy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var to))
                {
                    throw new FormatException("Формат для даты: dd.mm.yy. Пример: 29.01.17");
                }

                var command = new StatsCommand(chatId, from, to);
                return command;
            }

            return new StatsCommand(chatId, StatsType.Today);
        }
    }
}
