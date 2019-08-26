using System;
using Telegram.Bot.Types;

namespace StatsBot.Entities
{
    public class MessageInfo
    {
        public string ChatId { get; set; }

        public string Date { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string MessageType { get; set; }

        public MessageInfo(Message message, DateTimeOffset date)
        {
            ChatId = $"{message.Chat.Id:D}";
            UserId = message.From.Id.ToString("D");
            UserName = message.From.Username;
            FirstName = message.From.FirstName;
            LastName = message.From.LastName;
            MessageType = message.Type.ToString("G");
            Date = date.ToString("yyyy/MM/dd");
        }
    }
}
