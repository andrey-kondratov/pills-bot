using System;
using System.Collections.Generic;
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

namespace PillsBot.Server;

internal sealed class AzureOpenAIMessageProvider(IOptions<PillsBotOptions> options,
    ILogger<AzureOpenAIMessageProvider> logger,
    ConfigurationMessageProvider configurationMessageProvider,
    IChatCompletionService chatCompletionService) : IMessageProvider
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly PillsBotOptions _options = options.Value;
    private readonly ILogger<AzureOpenAIMessageProvider> _logger = logger;
    private readonly ConfigurationMessageProvider _configurationMessageProvider = configurationMessageProvider;
    private readonly IChatCompletionService _chatCompletionService = chatCompletionService;
    private readonly Queue<Choice> _choices = new();

    private OpenAIPromptExecutionSettings ExecutionSettings => new()
    {
        ChatSystemPrompt = """
            You are a smart assistant that reminds pet owners when it is time to give a pill. 

            A reminder is a chat message with a short text (at most 5 words), an acknowledgement button with another text (at most 3 words), and an appreciation message (at most 3 words).

            Upon receiving the reminder, owners will give the pill to their pet and confirm it by clicking the button. The button will then hide the reminder and show the appreciation message.
            """,
        MaxTokens = _options.AI.MaxTokens
    };

    private string Prompt => $"""
        Generate {_options.AI.ChoicesCount} unique chat messages, each in one of these languages: {_options.AI.Languages}.

        # Pet

        Names: {_options.AI.PetNames}.
        Gender: {_options.AI.PetGender}.

        # Output format
        
        JSON array. Field `r` for the reminder, `b` for the button, and `a` for the appreciation message. Return the raw JSON, without enclosing quotes.
        Always make sure the name is in the correct case, gender, and transliteration.
        """;

    public async Task<(string reminder, string button, string appreciation)> GetMessage(CancellationToken cancellationToken = default)
    {
        if (_choices.TryDequeue(out Choice result))
        {
            return (result.Reminder, result.Button, result.Appreciation);
        }

        try
        {
            foreach (Choice choice in await GetNewChoices(cancellationToken))
            {
                _choices.Enqueue(choice);
            }

            result = _choices.Dequeue();
            return (result.Reminder, result.Button, result.Appreciation);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error getting new messages from AI. Defaulting to the message in configuration.");

            return await _configurationMessageProvider.GetMessage(cancellationToken);
        }
    }

    private async Task<IEnumerable<Choice>> GetNewChoices(CancellationToken cancellationToken)
    {
        ChatMessageContent content = await _chatCompletionService
            .GetChatMessageContentAsync(Prompt, ExecutionSettings, cancellationToken: cancellationToken);

        string json = content.ToString();

        Choice[] choices = JsonSerializer.Deserialize<Choice[]>(json, JsonSerializerOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize choices as JSON. Raw response from the LLM: {json}.");

        new Random().Shuffle(choices);
        return choices;
    }

    internal record Choice
    {
        [JsonPropertyName("r")]
        public string Reminder { get; init; }

        [JsonPropertyName("b")]
        public string Button { get; init; }

        [JsonPropertyName("a")]
        public string Appreciation { get; init; }
    }
}
