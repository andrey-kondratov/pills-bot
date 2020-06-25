using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PillsBot.Server
{
    internal class BotService : IHostedService
    {
        private readonly ILogger<BotService> _logger;
        private readonly IMessenger _messenger;

        public BotService(ILogger<BotService> logger, IMessenger messenger)
        {
            _logger = logger;
            _messenger = messenger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting bot...");

            try
            {
                await _messenger.Start(cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(EventIds.BotStartupFailed, exception, "Failed to start messenger");
                return;
            }
            
            _logger.LogInformation("Bot started.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping bot");

            try
            {
                await _messenger.Stop(cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(EventIds.BotShutdownFailed, exception, "Failed to stop the messenger");
                return;
            }

            _logger.LogInformation("Bot stopped.");
        }
    }
}
