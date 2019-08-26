using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StatsBot.Commands;
using StatsBot.Gateways;
using StatsBot.Managers;
using Telegram.Bot.Types;

namespace StatsBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IStatsManager _statsManager;
        private readonly IStatsCommandBuilder _statsCommandBuilder;
        private readonly ITelegramGateway _telegramGateway;
        private readonly ILogger<MessagesController> _log;

        public MessagesController(IStatsManager statsManager,
            IStatsCommandBuilder statsCommandBuilder,
            ITelegramGateway telegramGateway,
            ILogger<MessagesController> log)
        {
            _statsManager = statsManager;
            _statsCommandBuilder = statsCommandBuilder;
            _telegramGateway = telegramGateway;
            _log = log;
        }

        [HttpPost]
        public async Task Post(Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            if (message == null)
            {
                _log.LogError("Message is null");
                return;
            }

            if (!Commands.Commands.IsCommand(message))
            {
                try
                {
                    await _statsManager.AddToStatsAsync(update, cancellationToken);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Add message to stat exception");
                }
                return;
            }

            switch (message.Text)
            {
                case var text when Commands.Commands.IsStart(text):
                    await _telegramGateway.SendMessageAsync(message.Chat.Id, Consts.Usage, cancellationToken);
                    break;

                case var text when Commands.Commands.IsStats(text):
                    await ReplyWithStatsAsync(message.Chat.Id, text, cancellationToken);
                    break;

                case var text when Commands.Commands.IsRules(text):
                    await _telegramGateway.SendMessageAsync(message.Chat.Id, Consts.Rules, cancellationToken);
                    break;

                default:
                    _log.LogInformation("Unknown command");
                    break;
            }
        }

        private async Task ReplyWithStatsAsync(long chatId, string text, CancellationToken cancellationToken)
        {
            var statsString = string.Empty;
            try
            {
                var command = _statsCommandBuilder.BuildFromString(chatId, text);

                statsString = await _statsManager.GetStatsStringAsync(command, cancellationToken);
            }
            catch (FormatException ex)
            {
                await _telegramGateway.SendMessageAsync(chatId, ex.Message, cancellationToken);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Stats command parsing exception");
            }

            if (!string.IsNullOrEmpty(statsString))
                await _telegramGateway.SendMessageAsync(chatId, statsString, cancellationToken);
        }
    }
}
