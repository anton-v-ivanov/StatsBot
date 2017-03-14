using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StatsBot.Entities;

namespace StatsBot.Repos
{
	internal interface IStatsRepo
	{
		Task EnsureSenderExists(MessageInfo messageInfo);

		Task AddMessage(long chatId, MessageInfo messageInfo, DateTime date);

		Task<IEnumerable<MessageInfo>> GetStats(long chatId, StatsCommand command);

		Task<IEnumerable<long>> GetChatsIds();

		Task<int> GetUserId(string userName);
	}
}