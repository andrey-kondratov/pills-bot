using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace PillsBot.Server.TextGeneration;

internal sealed class ConfigurationMessageProvider(IOptions<PillsBotOptions> options) : IMessageProvider
{
    private readonly IOptions<PillsBotOptions> _options = options;

    public Task<(string reminder, string button, string appreciation)> GetMessage(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((_options.Value.Reminder.Message, "Pill given!", "🐱"));
    }
}
