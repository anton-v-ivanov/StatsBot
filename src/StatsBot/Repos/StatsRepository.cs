using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using StatsBot.Entities;
using Telegram.Bot.Types.Enums;

namespace StatsBot.Repos
{
    public class StatsRepository : IStatsRepository
    {
        private readonly CollectionReference _messagesCollection;
        private readonly CollectionReference _usersCollection;

        public StatsRepository(FirestoreDb db)
        {
            // messages/chatId/date/participantId  { MessageType : counter }
            _messagesCollection = db.Collection("messages");
            // users/participantId { }
            _usersCollection = db.Collection("users");
        }

        public async Task SaveAsync(MessageInfo info, CancellationToken cancellationToken)
        {
            var messageRef = _messagesCollection.Document(info.ChatId).Collection(info.Date).Document(info.UserId);
            var messageSnapshot = await messageRef.GetSnapshotAsync(cancellationToken);
            if (!messageSnapshot.Exists)
            {
                await _usersCollection.Document(info.UserId)
                    .SetAsync(
                        new
                        {
                            info.FirstName,
                            info.LastName,
                            info.UserName
                        },
                        null,
                        cancellationToken);

                var update = new Dictionary<string, object> {{ info.MessageType, 1 }};
                await messageRef.SetAsync(update, null, cancellationToken);
            }
            else
            {
                await messageRef.UpdateAsync(info.MessageType, FieldValue.Increment(1), Precondition.None,
                    cancellationToken);
            }
        }

        public async Task<Stats> GetStatsAsync(StatsCommand command, CancellationToken cancellationToken)
        {
            var stats = new Stats();
            var perUserStat = new Dictionary<int, int>();
            foreach (var day in EachDay(command.From, command.To))
            {
                var daySnapshot = await _messagesCollection.Document(command.ChatId)
                    .Collection(day.ToString("yyyy/MM/dd"))
                    .GetSnapshotAsync(cancellationToken);

                foreach (var user in daySnapshot.Documents)
                {
                    var userId = Convert.ToInt32(user.Id);
                    var fields = user.ConvertTo<Dictionary<string, int>>();
                    foreach (var (messageType, count) in fields)
                    {
                        var type = Enum.Parse<MessageType>(messageType);
                        if (!stats.ChatStats.ContainsKey(type))
                            stats.ChatStats.Add(type, count);
                        else
                            stats.ChatStats[type] += count;

                        if (!perUserStat.ContainsKey(userId))
                            perUserStat.Add(userId, count);
                        else
                            perUserStat[userId] += count;

                        if(!stats.WeekStats.ContainsKey(day.DayOfWeek))
                            stats.WeekStats.Add(day.DayOfWeek, count);
                        else
                            stats.WeekStats[day.DayOfWeek] += count;
                    }
                }
            }

            var orderedStats = perUserStat.OrderByDescending(s => s.Value);
            foreach (var (userId, count) in orderedStats)
            {
                var userSnapshot = await _usersCollection.Document(userId.ToString("D")).GetSnapshotAsync(cancellationToken);
                userSnapshot.TryGetValue<string>("FirstName", out var firstName);
                userSnapshot.TryGetValue<string>("LastName", out var lastName);
                userSnapshot.TryGetValue<string>("UserName", out var userName);
                var userInfo = new UserInfo(userId, userName, firstName, lastName);
                stats.UserStats.Add(userInfo, count);
            }
            return stats;
        }

        public async Task<IEnumerable<long>> GetChatsIdsAsync(CancellationToken cancellationToken)
        {
            var collectionSnapshot = await _messagesCollection.ListDocumentsAsync().ToArray(cancellationToken);
            return collectionSnapshot.Select(d => Convert.ToInt64(d.Id));
        }

        private static IEnumerable<DateTime> EachDay(DateTimeOffset from, DateTimeOffset to)
        {
            for (var day = from.Date; day.Date <= to.Date; day = day.AddDays(1))
                yield return day;
        }
    }
}
