using Autofac;
using FluentScheduler;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using TlenBot.Jobs;
using TlenBot.Managers;
using TlenBot.Repos;

namespace TlenBot.Configuration
{
    public class BotInitModule : Module
    {
        private readonly IConfigurationRoot _configuration;

        public BotInitModule(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TelegramBotClient>()
                .As<ITelegramBotClient>()
                .WithParameter("token", _configuration["TelegramToken"]);

            builder.RegisterType<StatsRepo>()
                .As<IStatsRepo>()
                .WithParameter("connectionString", _configuration.GetConnectionString("DbConnection"));

            builder.RegisterType<StatsManager>().As<IStatsManager>();
            builder.RegisterType<StatsCommandBuilder>().As<IStatsCommandBuilder>();
            builder.RegisterType<TimeZoneManager>().As<ITimeZoneManager>();
            builder.RegisterType<BotManager>().As<IBotManager>();
            builder.RegisterType<ScheduleManager>().As<IScheduleManager>();

            builder.RegisterType<DailyStatsJob>().Keyed<IJob>("daily");
            builder.RegisterType<WeeklyStatsJob>().Keyed<IJob>("weekly");
        }
    }
}