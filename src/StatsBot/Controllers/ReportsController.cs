using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StatsBot.Entities;
using StatsBot.Gateways;
using StatsBot.Managers;

namespace StatsBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IStatsManager _statsManager;
        private readonly ITelegramGateway _telegramGateway;
        private readonly ILogger<ReportsController> _log;

        public ReportsController(IStatsManager statsManager,
            ITelegramGateway telegramGateway,
            ILogger<ReportsController> log)
        {
            _statsManager = statsManager;
            _telegramGateway = telegramGateway;
            _log = log;
        }

        [HttpGet("daily")]
        public async Task SendDailyStats(CancellationToken cancellationToken)
        {
            _log.LogInformation($"Daily stats triggered at: {DateTimeOffset.UtcNow}");
            var chats = await _statsManager.GetChatsIdsAsync(cancellationToken);

            foreach (var chatId in chats)
            {
                _log.LogInformation($"Daily stats: processing chatId: {chatId}");
                var statsString = await _statsManager.GetStatsStringAsync(new StatsCommand(chatId,
                        StatsType.Today),
                    cancellationToken);

                await _telegramGateway.SendMessageAsync(chatId, statsString, cancellationToken);

                _log.LogInformation($"Daily stats sent to chatId: {chatId}");
            }
        }

        [HttpGet("weekly")]
        public async Task SendWeeklyStats(CancellationToken cancellationToken)
        {
            _log.LogInformation($"Weekly stats triggered at: {DateTimeOffset.UtcNow}");
            var chats = await _statsManager.GetChatsIdsAsync(cancellationToken);

            foreach (var chatId in chats)
            {
                _log.LogInformation($"Weekly stats: processing chatId: {chatId}");
                var statsString = await _statsManager.GetStatsStringAsync(new StatsCommand(chatId,
                        StatsType.Week),
                    cancellationToken);
                await _telegramGateway.SendMessageAsync(chatId, statsString, cancellationToken);

                _log.LogInformation($"Weekly stats sent to chatId: {chatId}");
            }
        }
    }
}
