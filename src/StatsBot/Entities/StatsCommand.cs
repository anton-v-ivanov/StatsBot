using System;

namespace TlenBot.Entities
{
	public class StatsCommand
	{
		public DateTime FromDate { get; }

		public DateTime ToDate { get; }

		public string UserName { get; set; }

		public int UserId { get; set; }

		public StatsType Type { get; }

        public int ChatId { get; set; }

		public StatsCommand(DateTime from, DateTime to, StatsType type)
		{
			FromDate = from;
			ToDate = to;
			Type = type;
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
					return $"с {FromDate.Date:dd.MM.yy} по {ToDate.Date:dd.MM.yy}";
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}