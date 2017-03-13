using System;
using System.Runtime.Loader;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using TlenBot.Managers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using TlenBot.Entities;
using TlenBot.Repos;
using Microsoft.Extensions.Logging;

namespace TlenBot
{
    class Program
    {
        private static TelegramBotClient _bot;
	    private static IStatsManager _manager;
        private static ILogger _logger;

	    static void Main(string[] args)
        {
			var configuration = new ConfigurationBuilder()
				.Add(new JsonConfigurationSource
					{
						Path = "config.json",
						Optional = false,
						ReloadOnChange = true
					})
				.Build();
            
            var loggerFactory = new LoggerFactory().AddConsole().AddFile("Logs/tlenbot-{Date}.log");
            _logger = loggerFactory.CreateLogger<Program>();

            _bot = new TelegramBotClient(configuration["TelegramToken"]);
            _bot.OnMessage += BotOnMessageReceived;
            _bot.OnReceiveError += BotOnReceiveError;
            _logger.LogInformation("Bot started");
			
			IStatsRepo repo = new StatsRepo(configuration.GetConnectionString("DbConnection"));
			_manager = new StatsManager(repo);
			_manager.OnTimeToSendStats += SendDailyStats;

			_bot.StartReceiving();

            AssemblyLoadContext.Default.Unloading += MethodInvokedOnSigTerm;

            while (true)
            {

            }
        }

	    private static void MethodInvokedOnSigTerm(AssemblyLoadContext obj)
        {
            _logger.LogInformation("Bot stopped");

            _bot.StopReceiving();
        }

	    private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            _logger.LogError(receiveErrorEventArgs.ApiRequestException.Message);
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
	        var message = messageEventArgs.Message;

	        if (message == null)
		        return;

	        if (message.Type != MessageType.TextMessage || !Commands.IsCommand(message.Text))
	        {
		        try
		        {
			        await _manager.AddToStats(message);
		        }
		        catch (Exception ex)
		        {
                    _logger.LogError(ex.Message);
		        }
		        return;
	        }

			if (message.Text.StartsWith(Commands.Start))
	        {
		        BotReply(message.Chat.Id, Consts.Usage);
		        return;
	        }

			if (message.Text.StartsWith(Commands.Stats))
			{
				var statsString = string.Empty;
				try
				{
					var command = StatsCommand.FromString(message.Text);
					statsString = await _manager.GetStatsString(message.Chat.Id, command);
				}
				catch (FormatException ex)
				{
					BotReply(message.Chat.Id, ex.Message);
				}
		        catch (Exception ex)
		        {
                    _logger.LogError(ex.Message);
                }

				if(!string.IsNullOrEmpty(statsString))
					BotReply(message.Chat.Id, statsString);

				return;
	        }

			if (message.Text.StartsWith(Commands.Rules))
	        {
		        BotReply(message.Chat.Id, Consts.Rules);
				return;
	        }

			_logger.LogWarning("Unknown command");
        }

	    private static void SendDailyStats(long chatId, string statsString)
		{
			BotReply(chatId, statsString);
		}

		private static async void BotReply(long chatId, string text)
		{
			try
			{
				await _bot.SendTextMessageAsync(chatId, text);
			}
			catch (Exception ex)
			{
                _logger.LogError(ex.Message);
            }
		}
	}
}
