namespace StatsBot.Entities
{
    public class Chat
    {
        public Chat(long chatId) => ChatId = chatId;

        public Chat() { }

        public long ChatId { get; set; }
    }
}
