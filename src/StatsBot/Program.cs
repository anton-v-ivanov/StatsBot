using System;
using System.Runtime.Loader;
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

            AssemblyLoadContext.Default.Unloading += MethodInvokedOnSigTerm;

            while (true)
            {
                if (Console.Read() != 'q')
                    continue;

                Log.Information("Exit requested");
                break;
            }
        }

        private static void Init()
        {
            var botManager = _container.Resolve<IBotManager>();
            botManager.Init();

            _container.Resolve<IScheduleManager>().Init();

            botManager.Start();
        }

        private static void MethodInvokedOnSigTerm(AssemblyLoadContext obj)
        {
            _container.Resolve<IBotManager>().Stop();
        }
    }
}
