using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PillsBot.Server.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PillsBot.Server
{
    internal class TelegramMessenger(ILogger<TelegramMessenger> logger,
        ITelegramClientFactory clientFactory, IOptions<PillsBotOptions> options)
        : IMessenger, IUpdateHandler
    {
        private static readonly ReceiverOptions ReceiverOptions = new()
        {
            AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery]
        };

        private readonly ILogger<TelegramMessenger> _logger = logger;
        private readonly PillsBotOptions _options = options.Value;
        private readonly ITelegramClientFactory _clientFactory = clientFactory;
        private ITelegramBotClient _client;

        public async Task Start(CancellationToken cancellationToken = default)
        {
            _client = await _clientFactory.Create(_options.Connection.ApiToken, cancellationToken);

            // webhooks not supported
            WebhookInfo webhookInfo = await _client.GetWebhookInfoAsync(cancellationToken);
            if (!string.IsNullOrEmpty(webhookInfo.Url))
            {
                _logger.LogWarning("A webhook is set up on the server. Deleting...");
                await _client.DeleteWebhookAsync(true, cancellationToken);
            }

            _client.StartReceiving(this, ReceiverOptions, cancellationToken);
            _logger.LogInformation("Started receiving updates.");
        }

        public async Task Notify(string message, CancellationToken cancellationToken = default)
        {
            ChatId chatId = _options.Connection.ChatId ??
                throw new InvalidOperationException("Chat id not configured");

            IReplyMarkup replyMarkup = GetReplyMarkup();

            _logger.LogInformation("Sending message: {Message} to chat {ChatId}", message, chatId);
            await _client.SendTextMessageAsync(chatId, message, replyMarkup: replyMarkup,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Message sent.");
        }

        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            return update.Type switch
            {
                UpdateType.CallbackQuery => OnCallbackQuery(update.CallbackQuery, cancellationToken),
                UpdateType.Message => OnClientMessage(update.Message),
                _ => throw new InvalidEnumArgumentException("UpdateType", (int)update.Type, typeof(UpdateType))
            };
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Polling error.");
            return Task.CompletedTask;
        }

        private async Task OnCallbackQuery(CallbackQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInformation("A callback query received: {@CallbackQuery}", query);

            long chatId = query.Message.Chat.Id;
            if (chatId != _options.Connection.ChatId)
            {
                _logger.LogWarning("Unexpected chat id in callback query: {@CallbackQuery}", query);
                return;
            }

            // fire message removal
            await _client.DeleteMessageAsync(chatId, query.Message.MessageId, cancellationToken);

            // fire callback
            await _client.AnswerCallbackQueryAsync(query.Id, "🐱", cancellationToken: cancellationToken);
        }

        private Task OnClientMessage(Message message)
        {
            _logger.LogInformation("A message received: {@Message}", message);
            return Task.CompletedTask;
        }

        private static InlineKeyboardMarkup GetReplyMarkup()
        {
            var inlineKeyboardButton = InlineKeyboardButton.WithCallbackData("Eaten!", "roger");
            var result = new InlineKeyboardMarkup(inlineKeyboardButton);
            return result;
        }
    }
}
