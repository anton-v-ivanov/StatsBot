using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TlenBot.Entities;
using TlenBot.Repos;

namespace TlenBot.Managers
{
    public class StatsManager : IStatsManager
    {
        private readonly IStatsRepo _repository;
        private readonly ITimeZoneManager _timeZoneManager;

        public StatsManager(IStatsRepo repository, ITimeZoneManager timeZoneManager)
        {
            _repository = repository;
            _timeZoneManager = timeZoneManager;
        }

        public async Task AddToStats(Message message)
        {
            var info = new MessageInfo(message);
            await _repository.EnsureSenderExists(info.User);
            var date = _timeZoneManager.GetMoscowNowDate();
            await _repository.AddMessage(message.Chat.Id, info, date);
        }

        public async Task<string> GetStatsString(long chatId, StatsCommand command)
        {
            var sb = new StringBuilder();
            if (command.UserId == 0 && !string.IsNullOrEmpty(command.UserName))
            {
                command.UserId = await _repository.GetUserId(command.UserName);
            }

            var stats = await _repository.GetStats(chatId, command);

            var period = command.GetStringTypeModifier();
            if (stats.Count == 0)
                return string.Format(@"{0} все молчали ¯\_(ツ)_/¯", period);

            sb.AppendLine($"Топ тунеядцев {period}");

            foreach (var stat in stats.UserStats)
            {
                sb.AppendLine($"{stat.Key.GetName()}: {stat.Value}");
            }

            sb.AppendLine();

            var othersCount = 0;
            foreach (var chatStat in stats.ChatStats)
            {
                switch (chatStat.Key)
                {
                    case MessageType.TextMessage:
                        sb.AppendLine($"Текстовых сообщений: {chatStat.Value}");
                        break;
                    case MessageType.PhotoMessage:
                        sb.AppendLine($"Фоток: {chatStat.Value}");
                        break;
                    case MessageType.AudioMessage:
                        sb.AppendLine($"Аудио: {chatStat.Value}");
                        break;
                    case MessageType.VideoMessage:
                        sb.AppendLine($"Видео: {chatStat.Value}");
                        break;
                    case MessageType.VoiceMessage:
                        sb.AppendLine($"Голосовых сообщений: {chatStat.Value}");
                        break;
                    case MessageType.StickerMessage:
                        sb.AppendLine($"Стикеров: {chatStat.Value}");
                        break;
                    case MessageType.DocumentMessage:
                        sb.AppendLine($"Гифки, документы: {chatStat.Value}");
                        break;
                    case MessageType.LocationMessage:
                        sb.AppendLine($"Локации: {chatStat.Value}");
                        break;
                    default:
                        othersCount += chatStat.Value;
                        break;
                }
            }
            sb.AppendLine($"Других: {othersCount}");

            if (command.Type == StatsType.Week)
            {
                var perDateStats = await _repository.GetPerDayStats(chatId, command);
                sb.AppendLine();
                sb.AppendLine("По дням");
                foreach (var perDateStat in perDateStats)
                {
                    sb.AppendLine($"{perDateStat.Key.DayOfWeek}: {perDateStat.Value}");
                }
            }

            return sb.ToString();
        }
    }
}