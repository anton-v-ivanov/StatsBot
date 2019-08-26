using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace StatsBot.Gateways
{
    public class TelegramGateway : ITelegramGateway
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger<TelegramGateway> _log;

        public TelegramGateway(ITelegramBotClient telegramBotClient, ILogger<TelegramGateway> log)
        {
            _telegramBotClient = telegramBotClient;
            _log = log;
        }

        public async Task SendMessageAsync(long chatId, string text, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(text))
                return;

            try
            {
                await _telegramBotClient.SendTextMessageAsync(chatId,
                    text,
                    cancellationToken: cancellationToken);
            }
            catch (Exception e)
            {
                _log.LogError(e, $"Unable to send message to chat {chatId}");
            }
        }
    }
}
