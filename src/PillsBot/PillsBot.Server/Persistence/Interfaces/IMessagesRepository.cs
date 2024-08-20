using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PillsBot.Server.Persistence;

internal interface IMessagesRepository
{
    Task<(string reminder, string acknowledgement, string appreciation)[]> GetMessages(string language, CancellationToken cancellationToken = default);
    Task AddMessages(string language, IEnumerable<(string reminder, string acknowledgement, string appreciation)> messages, CancellationToken cancellationToken = default);
}
