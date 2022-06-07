using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PillsBot.Server.Configuration;

namespace PillsBot.Server
{
    internal class BotService : BackgroundService
    {
        private readonly ILogger<BotService> _logger;
        private readonly IMessenger _messenger;
        private readonly PillsBotOptions _options;

        public BotService(ILogger<BotService> logger, IMessenger messenger, IOptions<PillsBotOptions> options)
        {
            _logger = logger;
            _messenger = messenger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting bot...");

            try
            {
                await _messenger.Start(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(EventIds.BotStartupFailed, exception, "Failed to start messenger");
                return;
            }

            _logger.LogInformation("Bot started.");

            DateTime begins = _options.Reminder.Begins;
            TimeSpan interval = _options.Reminder.Interval;
            string message = _options.Reminder.Message;

            DateTime next = GetNext(begins, interval);
            _logger.LogInformation("Next reminder comes off at {Next}", next);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (next <= DateTime.UtcNow)
                {
                    await _messenger.Notify(message, stoppingToken);

                    next = GetNext(begins, interval);
                    _logger.LogInformation("Next reminder comes off at {Next}", next);
                }

                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation("Stopping bot");

            try
            {
                await _messenger.Stop(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(EventIds.BotShutdownFailed, exception, "Failed to stop the messenger");
                return;
            }

            _logger.LogInformation("Bot stopped.");
        }

        private DateTime GetNext(DateTime begins, TimeSpan interval)
        {
            var now = DateTime.UtcNow;
            if (now < begins)
            {
                return begins;
            }

            var current = begins;
            while (current < now)
            {
                current = current.Add(interval);
            }

            return current;
        }
    }
}
