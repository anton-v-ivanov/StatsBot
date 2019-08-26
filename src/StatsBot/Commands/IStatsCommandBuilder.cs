using StatsBot.Entities;

namespace StatsBot.Commands
{
    public interface IStatsCommandBuilder
    {
        StatsCommand BuildFromString(long chatId, string text);
    }
}