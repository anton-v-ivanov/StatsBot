using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TlenBot.Entities;

namespace TlenBot.Repos
{
    public interface IStatsRepo
    {
        Task EnsureSenderExists(UserInfo user);

        Task AddMessage(long chatId, MessageInfo messageInfo, DateTime date);

        Task<Stats> GetStats(long chatId, StatsCommand command);

        Task<IEnumerable<long>> GetChatsIds();

        Task<int> GetUserId(string userName);

        Task<Dictionary<DateTime, int>> GetPerDayStats(long chatId, StatsCommand command);
    }
}