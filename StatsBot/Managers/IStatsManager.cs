using System;
using System.Threading.Tasks;
using StatsBot.Entities;
using Telegram.Bot.Types;

namespace StatsBot.Managers
{
	internal interface IStatsManager
	{
		Task AddToStats(Message message);

		Task<string> GetStatsString(long chatId, StatsCommand command);

		event Action<long, string> OnTimeToSendStats;
	}
}