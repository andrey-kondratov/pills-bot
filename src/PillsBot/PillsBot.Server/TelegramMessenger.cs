using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PillsBot.Server.Configuration;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PillsBot.Server
{
    internal class TelegramMessenger : IMessenger
    {
        private static readonly UpdateType[] AllowedUpdates = { UpdateType.Message, UpdateType.CallbackQuery };

        private readonly ILogger<TelegramMessenger> _logger;
        private readonly PillsBotOptions _options;
        private readonly ITelegramClientFactory _clientFactory;
        private ITelegramBotClient _client;
        private CancellationToken _cancellationToken;

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
            _client.OnCallbackQuery += OnCallbackQuery;
            _client.StartReceiving(AllowedUpdates, cancellationToken);
            _logger.LogInformation("Started receiving updates.");

            _cancellationToken = cancellationToken;
        }

        public Task Stop(CancellationToken cancellationToken = default)
        {
            _client.StopReceiving();
            _client.OnMessage -= OnClientMessage;
            _client.OnCallbackQuery -= OnCallbackQuery;

            _logger.LogInformation("Stopped receiving updates");

            return Task.CompletedTask;
        }

        public async Task Notify(string message, CancellationToken cancellationToken = default)
        {
            ChatId chatId = _options.Connection.ChatId ?? 
                throw new InvalidOperationException("Chat id not configured");

            IReplyMarkup replyMarkup = GetReplyMarkup();

            _logger.LogInformation("Sending message: {Message} to chat {ChatId}", message, chatId);
            await _client.SendTextMessageAsync(chatId, message, ParseMode.Default, 
                cancellationToken: cancellationToken, replyMarkup: replyMarkup);

            _logger.LogInformation("Message sent.");
        }

        private void OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            _logger.LogInformation("A callback query received: {@CallbackQuery}", e.CallbackQuery);

            var chatId = e.CallbackQuery.Message.Chat.Id;
            if (chatId != _options.Connection.ChatId)
            {
                _logger.LogWarning("Unexpected chat id in callback query: {@CallbackQuery}", e.CallbackQuery);
                return;
            }

            // fire message removal
            _client.DeleteMessageAsync(chatId, e.CallbackQuery.Message.MessageId, _cancellationToken);

            // fire callback
            _client.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "🐱", cancellationToken: _cancellationToken);
        }

        private void OnClientMessage(object sender, MessageEventArgs e)
        {
            _logger.LogInformation("A message received: {@Message}", e.Message);
        }

        private IReplyMarkup GetReplyMarkup()
        {
            var inlineKeyboardButton = new InlineKeyboardButton
            {
                CallbackData = "roger",
                Text = "Eaten!"
            };

            var result = new InlineKeyboardMarkup(inlineKeyboardButton);

            return result;
        }
    }
}
