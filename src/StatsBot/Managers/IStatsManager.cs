using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StatsBot.Entities;
using Telegram.Bot.Types;

namespace StatsBot.Managers
{
    public interface IStatsManager
    {
        Task AddToStatsAsync(Update update, CancellationToken cancellationToken);
        Task<string> GetStatsStringAsync(StatsCommand command, CancellationToken cancellationToken);
        Task<IEnumerable<long>> GetChatsIdsAsync(CancellationToken cancellationToken);
    }
}
