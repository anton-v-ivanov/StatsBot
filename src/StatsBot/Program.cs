using System;
using System.Threading;
using Autofac;
using TlenBot.Managers;
using Microsoft.Extensions.Configuration;
using Serilog;
using TlenBot.Configuration;

namespace TlenBot
{
    internal class Program
    {
        private static IContainer _container;

        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("./config.json", false)
                .Build();

            Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .CreateLogger();

            var builder = new ContainerBuilder();
            var initModule = new BotInitModule(configuration);
            builder.RegisterModule(initModule);
            _container = builder.Build();

            Init();

            Log.Information("Bot started. Press q to exit");
            
            var stopRequested = false;
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Log.Information("Exit requested");
                stopRequested = true;
            };

            while (!stopRequested)
            {
                Thread.Sleep(1000);
            }

            _container.Resolve<IBotManager>().Stop();
        }

        private static void Init()
        {
            var botManager = _container.Resolve<IBotManager>();
            botManager.Init();

            _container.Resolve<IScheduleManager>().Init();

            botManager.Start();
        }
    }
}
