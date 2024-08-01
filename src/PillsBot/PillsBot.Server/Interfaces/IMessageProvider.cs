using System.Threading;
using System.Threading.Tasks;

namespace PillsBot.Server;

internal interface IMessageProvider
{
    internal Task<string> GetMessage(CancellationToken cancellationToken = default);
}