using System.Threading;
using System.Threading.Tasks;

namespace StatsBot.Gateways
{
    public interface ITelegramGateway
    {
        Task SendMessageAsync(long chatId, string text, CancellationToken cancellationToken);
    }
}