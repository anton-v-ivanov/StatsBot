using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TlenBot.Entities
{
	public class MessageInfo
	{
		public int Counter { get; set; }
		public readonly MessageType MessageType;
        public UserInfo User { get; private set; }

		public MessageInfo()
		{
		}

		public MessageInfo(Message message)
		{
            User = new UserInfo(message.From.Id, message.From.Username, message.From.FirstName, message.From.LastName);
			MessageType = message.Type;
		}
	}
}