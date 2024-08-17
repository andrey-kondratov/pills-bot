using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PillsBot.Server.Chat;
using PillsBot.Server.Configuration;
using PillsBot.Server.TextGeneration;

namespace PillsBot.Server
{
    internal class BotService(ILogger<BotService> logger, IChatClient chatClient,
        IOptions<PillsBotOptions> options, IMessageProvider messageProvider)
        : BackgroundService
    {
        private readonly ILogger<BotService> _logger = logger;
        private readonly IChatClient _chatClient = chatClient;
        private readonly PillsBotOptions _options = options.Value;
        private readonly IMessageProvider _messageProvider = messageProvider;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting bot...");

            try
            {
                await _chatClient.Start(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to start messenger");
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
                    (string reminder, string button, string appreciation) = await _messageProvider.GetMessage(stoppingToken);
                    await _chatClient.Notify(reminder, button, appreciation, stoppingToken);

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
