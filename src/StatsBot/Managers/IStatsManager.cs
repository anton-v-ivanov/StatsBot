using System.Threading.Tasks;
using Telegram.Bot.Types;
using TlenBot.Entities;

namespace TlenBot.Managers
{
    public interface IStatsManager
    {
        Task AddToStats(Message message);

        Task<string> GetStatsString(long chatId, StatsCommand command);
    }
}