using TlenBot.Entities;

namespace TlenBot.Managers
{
    public interface IStatsCommandBuilder
    {
        StatsCommand BuildFromString(string text);
    }
}