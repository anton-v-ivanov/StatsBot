using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace StatsBot.Entities
{
	internal class MessageInfo
	{
		public int Counter { get; set; }
		public int Id { get; set; }
		public string UserName { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public readonly MessageType MessageType;

		public MessageInfo()
		{
		}

		public MessageInfo(Message message)
		{
			Id = message.From.Id;
			UserName = message.From.Username;
			FirstName = message.From.FirstName;
			LastName = message.From.LastName;
			MessageType = message.Type;
		}

		public string GetName()
		{
			if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName) && string.IsNullOrEmpty(UserName))
				return Id.ToString();

			if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName))
				return UserName;

			var result = string.Empty;

			if (!string.IsNullOrEmpty(FirstName))
				result += FirstName;

			if (!string.IsNullOrEmpty(LastName))
				result += " " + LastName;

			return result;
		}
	}
}