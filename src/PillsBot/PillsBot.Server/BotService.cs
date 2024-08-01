using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PillsBot.Server.Configuration;

namespace PillsBot.Server
{
    internal class BotService(ILogger<BotService> logger, IMessenger messenger,
        IOptions<PillsBotOptions> options, IMessageProvider messageProvider)
        : BackgroundService
    {
        private readonly ILogger<BotService> _logger = logger;
        private readonly IMessenger _messenger = messenger;
        private readonly PillsBotOptions _options = options.Value;
        private readonly IMessageProvider _messageProvider = messageProvider;

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

            DateTime next = GetNext(begins, interval);
            _logger.LogInformation("Next reminder comes off at {Next}", next);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (next <= DateTime.Now)
                {
                    string message = await _messageProvider.GetMessage(stoppingToken);
                    await _messenger.Notify(message, stoppingToken);

                    next = GetNext(begins, interval);
                    _logger.LogInformation("Next reminder comes off at {Next}", next);
                }

                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation("Bot stopped.");
        }

        private static DateTime GetNext(DateTime begins, TimeSpan interval)
        {
            DateTime now = DateTime.Now;
            if (now < begins)
            {
                return begins;
            }

            DateTime current = begins;
            while (current < now)
            {
                current = current.Add(interval);
            }

            return current;
        }
    }
}
