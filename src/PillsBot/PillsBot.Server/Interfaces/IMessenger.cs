using System.Threading;
using System.Threading.Tasks;

namespace PillsBot.Server
{
    internal interface IMessenger
    {
        Task Start(CancellationToken cancellationToken = default);
        Task Stop(CancellationToken cancellationToken = default);
        Task Notify(string message, CancellationToken cancellationToken = default);
    }
}
