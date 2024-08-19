using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using PillsBot.Server.Configuration;
using PillsBot.Server.Persistence;
using Polly;
using Polly.Retry;

namespace PillsBot.Server.TextGeneration;

internal sealed class AzureOpenAIMessageProvider : IMessageProvider
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };

    private readonly PillsBotOptions _options;
    private readonly ILogger<AzureOpenAIMessageProvider> _logger;
    private readonly ConfigurationMessageProvider _configurationMessageProvider;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly Random _random = new();
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly IMessagesRepository _repository;
    private readonly IReadOnlyDictionary<string, string> _languages;

    public AzureOpenAIMessageProvider(IOptions<PillsBotOptions> options,
        ILogger<AzureOpenAIMessageProvider> logger,
        ConfigurationMessageProvider configurationMessageProvider,
        IChatCompletionService chatCompletionService,
        IMessagesRepository repository)
    {
        _options = options.Value;
        _logger = logger;
        _configurationMessageProvider = configurationMessageProvider;
        _chatCompletionService = chatCompletionService;
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                DelayGenerator = static args =>
                {
                    TimeSpan delay = args.AttemptNumber switch
                    {
                        0 => TimeSpan.Zero,
                        1 => TimeSpan.FromSeconds(1),
                        _ => TimeSpan.FromMinutes(1)
                    };

                    return new ValueTask<TimeSpan?>(delay);
                },
                OnRetry = args =>
                {
                    logger.LogWarning("OnRetry, Attempt: {AttemptNumber}", args.AttemptNumber);

                    return default;
                }
            })
            .Build();

        _repository = repository;

        _languages = options.Value.AI.Languages
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(input => (code: input, name: GetISOLanguageName(input)))
            .Where(pair => pair.name is not null)
            .ToDictionary(pair => pair.code, pair => pair.name!)
            .AsReadOnly();
    }

    private OpenAIPromptExecutionSettings ExecutionSettings => new()
    {
        ChatSystemPrompt = """
            You are a smart assistant that reminds pet owners when it is time to give a pill. 

            A reminder is a chat message with a short text (at most 5 words), an acknowledgement button with another text (at most 3 words), and an appreciation message (at most 3 words).

            Upon receiving the reminder, owners will give the pill to their pet and confirm it by clicking the button. The button will then hide the reminder and show the appreciation message.
            """,
        MaxTokens = _options.AI.MaxTokens
    };

    private string GetPrompt(string language)
    {
        return $$"""
        Generate {{_options.AI.ChoicesCount}} unique chat messages in this language: {{language}}.

        # Pet

        Names: {{_options.AI.PetNames}}.
        Gender: {{_options.AI.PetGender}}.

        # Output format
        
        JSON array. Field `r` for the reminder, `b` for the button, and `a` for the appreciation message. Return the raw JSON, without enclosing quotes.
        Always make sure the name is in the correct case, gender, and transliteration.
        """;
    }

    public async Task<(string reminder, string button, string appreciation)> GetMessage(CancellationToken cancellationToken = default)
    {
        // picking a language at random
        (string languageCode, string language) = _languages.ElementAt(_random.Next(_languages.Count));
        _logger.LogInformation("Picking language at random: {LanguageCode}, {Language}.", languageCode, language);

        // checking messages in the repository
        (string reminder, string acknowledgement, string appreciation)[] messages =
            await _repository.GetMessages(languageCode, cancellationToken);
        _logger.LogInformation("Number of messages in the repo with language code {LanguageCode}: {MessagesCount}.",
            languageCode, messages.Length);

        // filling the repository if empty
        if (messages.Length == 0)
        {
            _logger.LogInformation("No messages found for language {Language}, will ask AI to generate.", language);

            IEnumerable<Choice> choices;
            try
            {
                // get choices from the AI
                choices = await _resiliencePipeline.ExecuteAsync(token => GetNewChoices(language, token), cancellationToken);

                // store in the repository
                messages = choices.Select(choice => (choice.Reminder, choice.Button, choice.Appreciation)).ToArray();
                await _repository.AddMessages(languageCode, messages, cancellationToken);
                _logger.LogInformation("Saved {MessagesCount} messages with language code {LanguageCode} to the repository.",
                    messages.Length, languageCode);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error updating messages from AI. Defaulting to the message in configuration.");

                return await _configurationMessageProvider.GetMessage(cancellationToken);
            }
        }

        // pick a random message
        (string reminder, string acknowledgement, string appreciation) message = messages[_random.Next(messages.Length)];
        _logger.LogInformation("Picking message at random: {@Message}", message);

        return message;
    }

    private string? GetISOLanguageName(string input)
    {
        try
        {
            var culture = CultureInfo.GetCultureInfo(input, true);
            return culture.TwoLetterISOLanguageName;
        }
        catch (CultureNotFoundException exception)
        {
            _logger.LogWarning(exception, "Failed to find culture info for input: {Input}.", input);
            return default;
        }
    }

    private async ValueTask<IEnumerable<Choice>> GetNewChoices(string language, CancellationToken cancellationToken)
    {
        string prompt = GetPrompt(language);

        ChatMessageContent content;
        try
        {
            content = await _chatCompletionService
                .GetChatMessageContentAsync(prompt, ExecutionSettings, cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Error getting the chat message content.");
            throw new ChatCompletionException(exception);
        }

        string json = content.ToString();
        Choice[] choices = JsonSerializer.Deserialize<Choice[]>(json, JsonSerializerOptions)
            ?? throw new InvalidOperationException("Failed to deserialize choices as JSON.");

        return choices;
    }

    internal record Choice
    {
        [JsonPropertyName("r")]
        public required string Reminder { get; init; }

        [JsonPropertyName("b")]
        public required string Button { get; init; }

        [JsonPropertyName("a")]
        public required string Appreciation { get; init; }
    }
}
