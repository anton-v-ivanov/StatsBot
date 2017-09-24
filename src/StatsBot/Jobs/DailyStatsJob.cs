using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentScheduler;
using Serilog;
using TlenBot.Entities;
using TlenBot.Managers;
using TlenBot.Repos;

namespace TlenBot.Jobs
{
    public class DailyStatsJob: IJob
    {
        private readonly IStatsManager _manager;
        private readonly IStatsRepo _repo;
        private readonly IBotManager _botManager;
        private readonly ITimeZoneManager _timeZoneManager;

        public DailyStatsJob(IStatsManager manager, IStatsRepo repo, IBotManager botManager, ITimeZoneManager timeZoneManager)
        {
            _manager = manager;
            _repo = repo;
            _botManager = botManager;
            _timeZoneManager = timeZoneManager;
        }

        public void Execute()
        {
            ExecuteAsync().ConfigureAwait(false);
        }

        private async Task ExecuteAsync()
        {
            IEnumerable<long> chats = new List<long>();
            var now = DateTime.Now;
            try
            {
                chats = await _repo.GetChatsIds();
                now = _timeZoneManager.GetMoscowNowTime();
                if (now.Hour == 0)
                {
                    now = now.AddDays(-1);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Daily job get chats error: ");
            }

            foreach (var chatId in chats)
            {
                //if (Environment.GetEnvironmentVariable("Dev").Equals("true"))
                //    statsString = await _manager.GetStatsString(-165034900, new StatsCommand(now, now, StatsType.Today));
                //else
                var statsString = string.Empty;
                try
                {
                    statsString = await _manager.GetStatsString(chatId, new StatsCommand(now, now, StatsType.Today));
                }
                catch (Exception e)
                {
                    Log.Error(e, "Daily job get message error: ");
                }
                await _botManager.SendReply(chatId, statsString);
            }
        }
    }
}