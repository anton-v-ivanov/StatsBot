using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StatsBot.Commands;
using StatsBot.Gateways;
using StatsBot.Managers;
using StatsBot.Repos;
using Telegram.Bot;

namespace StatsBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(Configuration["TelegramToken"]));
            services.AddSingleton<ITelegramGateway, TelegramGateway>();
            services.AddSingleton<IStatsManager, StatsManager>();
            services.AddSingleton<IStatsRepository, StatsRepository>();
            services.AddSingleton<IStatsCommandBuilder, StatsCommandBuilder>();
            services.AddSingleton<ITimeZoneManager, TimeZoneManager>();
            services.AddSingleton(FirestoreDb.Create(Configuration["ProjectId"]));

            services.AddLogging();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
