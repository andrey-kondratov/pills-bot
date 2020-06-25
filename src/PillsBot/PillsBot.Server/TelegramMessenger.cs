using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PillsBot.Server.Configuration;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PillsBot.Server
{
    internal class TelegramMessenger : IMessenger
    {
        private static readonly UpdateType[] AllowedUpdates = { UpdateType.Message };

        private readonly ILogger<TelegramMessenger> _logger;
        private readonly PillsBotOptions _options;
        private readonly ITelegramClientFactory _clientFactory;
        private ITelegramBotClient _client;

        public TelegramMessenger(ILogger<TelegramMessenger> logger, ITelegramClientFactory clientFactory, IOptions<PillsBotOptions> options)
        {
            _logger = logger;
            _options = options.Value;
            _clientFactory = clientFactory;
        }

        public async Task Start(CancellationToken cancellationToken = default)
        {
            _client = await _clientFactory.Create(_options.Connection.ApiToken);

            // webhooks not supported
            WebhookInfo webhookInfo = await _client.GetWebhookInfoAsync(cancellationToken);
            if (!string.IsNullOrEmpty(webhookInfo.Url))
            {
                _logger.LogWarning("A webhook is set up on the server. Deleting...");
                await _client.DeleteWebhookAsync(cancellationToken);
            }

            _client.OnMessage += OnClientMessage;
            _client.StartReceiving(AllowedUpdates, cancellationToken);
            _logger.LogInformation("Started receiving updates.");
        }

        private void OnClientMessage(object sender, MessageEventArgs e)
        {
            _logger.LogInformation("A message received: {@Message}", e.Message);
        }

        public Task Stop(CancellationToken cancellationToken = default)
        {
            _client.StopReceiving();
            _client.OnMessage -= OnClientMessage;
            _logger.LogInformation("Stopped receiving updates");

            return Task.CompletedTask;
        }
    }
}
