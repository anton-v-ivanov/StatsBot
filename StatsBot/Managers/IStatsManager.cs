using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TlenBot.Entities;

namespace TlenBot.Managers
{
	internal interface IStatsManager
	{
		Task AddToStats(Message message);

		Task<string> GetStatsString(long chatId, StatsCommand command);

		event Action<long, string> OnTimeToSendStats;
	}
}