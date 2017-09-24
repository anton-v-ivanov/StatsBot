using System.Collections.Generic;
using Telegram.Bot.Types.Enums;

namespace TlenBot.Entities
{
    public class Stats
    {
        public Dictionary<UserInfo, int> UserStats { get; set; }
        public Dictionary<MessageType, int> ChatStats { get; set; }

        public int Count => UserStats.Count;

        public Stats()
        {
            UserStats = new Dictionary<UserInfo, int>();
            ChatStats = new Dictionary<MessageType, int>();
        }
    }
}