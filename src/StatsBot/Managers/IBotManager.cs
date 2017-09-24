using System.Threading.Tasks;

namespace TlenBot.Managers
{
    public interface IBotManager
    {
        void Init();
        void Start();
        void Stop();
        Task SendReply(long chatId, string text);
    }
}