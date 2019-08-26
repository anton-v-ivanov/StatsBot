using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StatsBot.Entities;
using StatsBot.Repos;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace StatsBot.Managers
{
    public class StatsManager : IStatsManager
    {
        private readonly IStatsRepository _repository;
        private readonly ITimeZoneManager _timeZoneManager;

        public StatsManager(IStatsRepository repository, ITimeZoneManager timeZoneManager)
        {
            _repository = repository;
            _timeZoneManager = timeZoneManager;
        }

        public Task AddToStatsAsync(Update update, CancellationToken cancellationToken)
        {
            var date = _timeZoneManager.GetMoscowDate(update.Message.Date);
            var info = new MessageInfo(update.Message, date);
            return _repository.SaveAsync(info, cancellationToken);
        }

        public async Task<string> GetStatsStringAsync(StatsCommand command, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();

            var stats = (await _repository.GetStatsAsync(command, cancellationToken));

            var period = command.GetStringTypeModifier();
            if (stats.Count == 0)
                return $@"{period} все молчали ¯\_(ツ)_/¯";

            sb.AppendLine($"Топ тунеядцев {period}");

            var place = 1;
            foreach (var (userInfo, count) in stats.UserStats)
            {
                sb.AppendLine($"{place++}. {userInfo}: {count}");
            }

            sb.AppendLine();

            var othersCount = 0;
            foreach (var (messageType, count) in stats.ChatStats)
            {
                switch (messageType)
                {
                    case MessageType.Text:
                        sb.AppendLine($"Текстовых сообщений: {count}");
                        break;
                    case MessageType.Photo:
                        sb.AppendLine($"Фоток: {count}");
                        break;
                    case MessageType.Audio:
                        sb.AppendLine($"Аудио: {count}");
                        break;
                    case MessageType.Video:
                        sb.AppendLine($"Видео: {count}");
                        break;
                    case MessageType.Voice:
                        sb.AppendLine($"Голосовых сообщений: {count}");
                        break;
                    case MessageType.Sticker:
                        sb.AppendLine($"Стикеров: {count}");
                        break;
                    case MessageType.Document:
                        sb.AppendLine($"Гифки, документы: {count}");
                        break;
                    case MessageType.Location:
                        sb.AppendLine($"Локации: {count}");
                        break;
                    case MessageType.ChatMembersAdded:
                        if(count > 0)
                            sb.AppendLine($"Добавлено в чатик: {count}");
                        break;
                    case MessageType.ChatMemberLeft:
                        sb.AppendLine($"Выпилились из чатика: {count} =(");
                        break;
                    default:
                        othersCount += count;
                        break;
                }
            }

            if (othersCount > 0)
                sb.AppendLine($"Других: {othersCount}");

            if (command.Type == StatsType.Week)
            {
                sb.AppendLine();
                sb.AppendLine("По дням");
                foreach (var (date, count) in stats.WeekStats)
                {
                    sb.AppendLine($"{date}: {count}");
                }
            }

            return sb.ToString();
        }

        public Task<IEnumerable<long>> GetChatsIdsAsync(CancellationToken cancellationToken) =>
            _repository.GetChatsIdsAsync(cancellationToken);
    }
}
