using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using StatsBot.Entities;
using StatsBot.Repos;

namespace StatsBot.Managers
{
	internal class StatsManager : IStatsManager
	{
		private readonly IStatsRepo _repository;
		private static Timer _timer;
		public event Action<long, string> OnTimeToSendStats;

		public StatsManager(IStatsRepo repository)
		{
			_repository = repository;
			_timer = new Timer(TimeToSendStats, null, 0, 1000);
		}

		public async Task AddToStats(Message message)
		{
			var info = new MessageInfo(message);
			await _repository.EnsureSenderExists(info);
            var date = TimeZoneManager.GetMoscowNowDate();
            await _repository.AddMessage(message.Chat.Id, info, date);
		}

		public async Task<string> GetStatsString(long chatId, StatsCommand command)
		{
			var sb = new StringBuilder();
			if (command.UserId == 0 && !string.IsNullOrEmpty(command.UserName))
			{
				command.UserId = await _repository.GetUserId(command.UserName);
			}

			var result = await _repository.GetStats(chatId, command);
			var stats = result.ToList();

			var period = command.GetStringTypeModifier();
			if (stats.Count == 0)
				return string.Format(@"{0} все молчали ¯\_(ツ)_/¯", period);

			sb.AppendLine($"Топ тунеядцев {period}");
			int textCount = 0, photoCount = 0, stickerCount = 0, videoCount = 0, audioCount = 0, voiceMessage = 0, otherCount = 0;
			var participants = new Dictionary<string, int>();

			foreach (var info in stats)
			{
				var name = info.GetName();
				if (participants.ContainsKey(name))
					participants[name] += info.Counter;
				else
					participants.Add(name, info.Counter);

				switch (info.MessageType)
				{
					case MessageType.TextMessage:
						textCount += info.Counter;
						break;
					case MessageType.PhotoMessage:
						photoCount += info.Counter;
						break;
					case MessageType.AudioMessage:
						audioCount += info.Counter;
						break;
					case MessageType.VideoMessage:
						videoCount += info.Counter;
						break;
					case MessageType.VoiceMessage:
						voiceMessage += info.Counter;
						break;
					case MessageType.StickerMessage:
						stickerCount += info.Counter;
						break;
					case MessageType.ServiceMessage:
						break;
					default:
						otherCount += info.Counter;
						break;
				}
			}

			foreach (var participant in participants)
			{
				sb.AppendLine($"{participant.Key}: {participant.Value}");
			}

			sb.AppendLine();
			sb.AppendLine($"Текстовых сообщений: {textCount}");
			sb.AppendLine($"Фоток: {photoCount}");
			sb.AppendLine($"Стикеров: {stickerCount}");
			sb.AppendLine($"Видео: {videoCount}");
			sb.AppendLine($"Аудио: {audioCount}");
			sb.AppendLine($"Голосовых сообщений: {voiceMessage}");
			sb.AppendLine($"Других: {otherCount}");

			return sb.ToString();
		}
		
		private async void TimeToSendStats(object state)
		{
			var now = TimeZoneManager.GetMoscowNowTime();
			if (now.Hour != 23 || now.Minute != 59 || now.Second != 59)
				return;

			var chats = await _repository.GetChatsIds();
			
			foreach (var chatId in chats)
			{
				var statsString = await GetStatsString(chatId, new StatsCommand(now, now, StatsType.Today));
				InvokeTimeToSendStats(chatId, statsString);
			}

		}

		protected virtual void InvokeTimeToSendStats(long chatId, string statsString)
		{
			OnTimeToSendStats?.Invoke(chatId, statsString);
		}
	}
}