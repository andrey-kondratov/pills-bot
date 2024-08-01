using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PillsBot.Server.Configuration;

namespace PillsBot.Server;

internal sealed class ConfigurationMessageProvider(IOptions<PillsBotOptions> options) : IMessageProvider
{
    private readonly IOptions<PillsBotOptions> _options = options;

    public Task<string> GetMessage(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_options.Value.Reminder.Message);
    }
}
