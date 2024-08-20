using System.Threading;
using System.Threading.Tasks;

namespace PillsBot.Server;

internal interface IChatClient
{
    Task Start(CancellationToken cancellationToken = default);
    Task Notify(string reminder, string button, string appreciation, CancellationToken cancellationToken = default);
}
