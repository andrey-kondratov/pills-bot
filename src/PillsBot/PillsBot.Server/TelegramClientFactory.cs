using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace PillsBot.Server
{
    public class TelegramClientFactory : ITelegramClientFactory
    {
        private readonly ILogger<TelegramClientFactory> _logger;

        public TelegramClientFactory(ILogger<TelegramClientFactory> logger)
        {
            _logger = logger;
        }

        public async Task<ITelegramBotClient> Create(string token, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating the client...");

            TelegramBotClient client;
            try
            {
                client = new TelegramBotClient(token);
            }
            catch (Exception exception)
            {
                _logger.LogError(EventIds.TelegramClientCreationFailed, exception, "Failed to create Telegram client");
                return null;
            }

            bool alive = await client.TestApiAsync(cancellationToken);
            if (!alive)
            {
                _logger.LogError(EventIds.TelegramClientCreationFailed, "Telegram client test failed");
                return null;
            }

            return client;
        }
    }
}
