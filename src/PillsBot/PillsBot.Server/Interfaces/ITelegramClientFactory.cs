using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace PillsBot.Server
{
    internal interface ITelegramClientFactory
    {
        Task<ITelegramBotClient> Create(string token, CancellationToken cancellationToken = default);
    }
}
