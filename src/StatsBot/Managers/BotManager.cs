using System;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace TlenBot.Managers
{
    public class BotManager: IBotManager
    {
        private readonly ITelegramBotClient _bot;
        private readonly IStatsManager _manager;
        private readonly IStatsCommandBuilder _statsCommandBuilder;

        public BotManager(ITelegramBotClient botClient, IStatsManager manager, IStatsCommandBuilder statsCommandBuilder)
        {
            _bot = botClient;
            _manager = manager;
            _statsCommandBuilder = statsCommandBuilder;
        }

        public void Init()
        {
            _bot.OnMessage += BotOnMessageReceived;
            _bot.OnReceiveError += BotOnReceiveError;
        }

        public void Start()
        {
            _bot.StartReceiving();
        }

        public void Stop()
        {
            Log.Information("Bot stopped");
            try
            {
                _bot?.StopReceiving();
            }
            catch
            {
                // ignored
            }
        }

        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
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
                    Log.Error(ex, "Add message to stat exception");
                }
                return;
            }

            if (message.Text.StartsWith(Commands.Start))
            {
                await SendReply(message.Chat.Id, Consts.Usage);
                return;
            }

            if (message.Text.StartsWith(Commands.Stats))
            {
                var statsString = string.Empty;
                try
                {
                    var command = _statsCommandBuilder.BuildFromString(message.Text);
                    var chatId = message.Chat.Id;
                    if (command.ChatId != 0)
                        chatId = command.ChatId;

                    statsString = await _manager.GetStatsString(chatId, command);
                }
                catch (FormatException ex)
                {
                    await SendReply(message.Chat.Id, ex.Message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Stats command parsing exception");
                }

                if (!string.IsNullOrEmpty(statsString))
                    await SendReply(message.Chat.Id, statsString);

                return;
            }

            if (message.Text.StartsWith(Commands.Rules))
            {
                await SendReply(message.Chat.Id, Consts.Rules);
                return;
            }

            Log.Warning("Unknown command");
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Log.Error(receiveErrorEventArgs.ApiRequestException.Message);
        }

        public async Task SendReply(long chatId, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            try
            {
                await _bot.SendTextMessageAsync(chatId, text);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Bot reply error");
            }
        }
    }
}