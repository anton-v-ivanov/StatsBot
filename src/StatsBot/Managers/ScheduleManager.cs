using System;
using Autofac;
using FluentScheduler;
using Serilog;

namespace TlenBot.Managers
{
    public class ScheduleManager : IScheduleManager
    {
        private readonly ITimeZoneManager _timeZoneManager;
        private readonly IJob _dailyJob;
        private readonly IJob _weeklyJob;

        public ScheduleManager(IComponentContext context, ITimeZoneManager timeZoneManager)
        {
            _timeZoneManager = timeZoneManager;
            _dailyJob = context.ResolveKeyed<IJob>("daily");
            _weeklyJob = context.ResolveKeyed<IJob>("weekly");
        }

        public void Init()
        {
            var registry = new Registry();
            registry.NonReentrantAsDefault();

            if (Consts.IsDev)
            {
                registry.Schedule(_dailyJob).NonReentrant().ToRunEvery(11).Seconds();
                registry.Schedule(_weeklyJob).NonReentrant().ToRunEvery(19).Seconds();
            }
            else
            {
                var timeToRunJobs = _timeZoneManager.GetMoscowEndOfDayInLocalTime();
                var hour = timeToRunJobs.Item1;
                var minute = timeToRunJobs.Item2;
                Log.Information("Stats jobs scheduled to {hour}:{minute}", hour, minute);

                registry.Schedule(_dailyJob).ToRunEvery(0).Days().At(timeToRunJobs.Item1, timeToRunJobs.Item2);
                registry.Schedule(_weeklyJob).ToRunEvery(0).Weeks().On(DayOfWeek.Sunday).At(timeToRunJobs.Item1, timeToRunJobs.Item2);
            }

            JobManager.Initialize(registry);
        }
    }
}