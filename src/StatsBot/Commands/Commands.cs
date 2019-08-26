using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace StatsBot.Commands
{
    public class Commands
    {
        internal const string Start = "/start";
        internal const string Stats = "/stat";
        internal const string Rules = "/rules";

        public static bool IsCommand(Message message) =>
            message.Type == MessageType.Text
            && (message.Text.StartsWith(Start) || message.Text.StartsWith(Stats) || message.Text.StartsWith(Rules));

        public static bool IsStart(string text) => text.StartsWith(Start);

        public static bool IsStats(string text) => text.StartsWith(Stats);

        public static bool IsRules(string text) => text.StartsWith(Rules);
    }
}
