using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StatsBot.Entities;

namespace StatsBot.Repos
{
    public interface IStatsRepository
    {
        Task SaveAsync(MessageInfo info, CancellationToken cancellationToken);
        Task<Stats> GetStatsAsync(StatsCommand command, CancellationToken cancellationToken);
        Task<IEnumerable<long>> GetChatsIdsAsync(CancellationToken cancellationToken);
    }
}
