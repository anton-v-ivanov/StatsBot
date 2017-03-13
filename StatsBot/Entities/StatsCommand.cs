using System;
using System.Globalization;

namespace TlenBot.Entities
{
	public class StatsCommand
	{
		public DateTime FromDate { get; set; }

		public DateTime ToDate { get; set; }

		public string UserName { get; set; }

		public int UserId { get; set; }

		public StatsType Type { get; set; }

		public StatsCommand(DateTime from, DateTime to, StatsType type)
		{
			FromDate = from;
			ToDate = to;
			Type = type;
		}

		public static StatsCommand FromString(string text)
		{
			// /stats 11.02.2017 12.03.2017 @tonageme
			// /stats
			// /stats сегодня
			// /stats вчера
			// /stats неделя
			// /stats месяц
			var tokens = text.Split(' ');
            var now = TimeZoneInfo.ConvertTime(DateTime.Now, Consts.MoscowTimeZone).Date;

            if (tokens.Length == 2)
			{
				var modifier = tokens[1];
				if(modifier.Equals("сегодня", StringComparison.OrdinalIgnoreCase))
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
				// /stats 11.02.2017 12.03.2017 @tonageme
				DateTime from;
				if (!DateTime.TryParseExact(tokens[1], "dd.MM.yy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out from))
				{
					throw new FormatException("Формат для даты: dd.mm.yy. Пример: 29.01.17");
				}

				DateTime to;
				if (!DateTime.TryParseExact(tokens[2], "dd.MM.yy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out to))
				{
					throw new FormatException("Формат для даты: dd.mm.yy. Пример: 29.01.17");
				}

				var command = new StatsCommand(from, to, StatsType.Period);
				if (tokens.Length > 3)
				{
					int id;
					if (int.TryParse(tokens[3], out id))
						command.UserId = id;
					else
						command.UserName = tokens[3].Replace("@", "");
				}

				return command;
			}

			return new StatsCommand(now, now, StatsType.Today);
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
					return $"с {FromDate.Date.ToString("dd.MM.yy")} по {ToDate.Date.ToString("dd.MM.yy")}";
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}