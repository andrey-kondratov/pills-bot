using System.Threading;
using System.Threading.Tasks;

namespace PillsBot.Server.TextGeneration;

internal interface IMessageProvider
{
    internal Task<(string reminder, string button, string appreciation)> GetMessage(CancellationToken cancellationToken = default);
}
