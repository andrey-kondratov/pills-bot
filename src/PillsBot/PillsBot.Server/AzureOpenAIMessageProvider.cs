using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using PillsBot.Server.Configuration;

namespace PillsBot.Server;

internal sealed class AzureOpenAIMessageProvider : IMessageProvider
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly PillsBotOptions _options;
    private readonly ILogger<AzureOpenAIMessageProvider> _logger;
    private readonly ConfigurationMessageProvider _configurationMessageProvider;
    private readonly Kernel _kernel;
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

    private string PromptTemplate => $"""
        Generate {_options.AI.ChoicesCount} unique chat messages, each in one of these languages: {_options.AI.Languages}.

        # Pet

        Names: {_options.AI.PetNames}.
        Gender: {_options.AI.PetGender}.

        # Output format
        
        JSON array. Field `r` for the reminder, `b` for the button, and `a` for the appreciation message. Return the raw JSON, without enclosing quotes.
        Always make sure the name is in the correct case, gender, and transliteration.
        """;

    public AzureOpenAIMessageProvider(IOptions<PillsBotOptions> options,
        ILogger<AzureOpenAIMessageProvider> logger,
        ConfigurationMessageProvider configurationMessageProvider)
    {
        _options = options.Value;
        _logger = logger;
        _configurationMessageProvider = configurationMessageProvider;

        string endpoint = _options.AI.Azure.Endpoint
            ?? throw new InvalidOperationException("Azure OpenAI endpoint is not configured.");
        string key = _options.AI.Azure.Key
            ?? throw new InvalidOperationException("Azure OpenAI key is not configured.");
        string deploymentName = _options.AI.Azure.DeploymentName
            ?? throw new InvalidOperationException("Azure OpenAI deployment name is not configured.");

        IKernelBuilder builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(deploymentName, endpoint, key);

        builder.Services.AddLogging(builder =>
            builder.AddConsole().SetMinimumLevel(_options.AI.LogLevel));

        _kernel = builder.Build();
    }

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
        FunctionResult result = await _kernel.InvokePromptAsync(PromptTemplate, new KernelArguments(ExecutionSettings), 
            cancellationToken: cancellationToken);

        string json = result.ToString();
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
