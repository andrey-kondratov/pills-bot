using System;
using System.Collections.Generic;
using System.Linq;
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
    private const char Separator = '!';

    private readonly PillsBotOptions _options;
    private readonly ILogger<AzureOpenAIMessageProvider> _logger;
    private readonly ConfigurationMessageProvider _configurationMessageProvider;
    private readonly Kernel _kernel;
    private readonly Queue<string> _messages = new();

    private OpenAIPromptExecutionSettings ExecutionSettings => new()
    {
        ChatSystemPrompt = "You are a creative veterinary assistant.",
        MaxTokens = _options.AI.MaxTokens
    };

    private string PromptTemplate => $$$"""
        Generate {{{_options.AI.ChoicesCount}}} short ({{{_options.AI.MaxWords}}} words max) unique messages reminding the owners that it is now time to give a pill. 
        Each message should be addressed to the humans who own the pet.
        Each message must end with an '{{{Separator}}}' sign. Output the messages in one row.
        Use these languages evenly: {{$languages}}.

        The cat's gender is {{$gender}}. The cat's names are {{$names}}. 
        You may include a single name in the message, but be sure to address the owners, not the cat. 
        Always make sure the name is in the correct case and gender.
        Transliterate the name if its alphabet differs from that of the message.
        """;

    private KernelArguments KernelArguments => new()
    {
        ["languages"] = _options.AI.Languages,
        ["names"] = _options.AI.PetNames,
        ["gender"] = _options.AI.PetGender
    };

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

    public async Task<string> GetMessage(CancellationToken cancellationToken = default)
    {
        if (_messages.TryDequeue(out string result))
        {
            return result;
        }

        try
        {
            foreach (string message in await GetNewMessages(cancellationToken))
            {
                _messages.Enqueue(message);
            }

            return _messages.Dequeue();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error getting new messages from AI. Defaulting to the message in configuration.");

            return await _configurationMessageProvider.GetMessage(cancellationToken);
        }
    }

    private async Task<IEnumerable<string>> GetNewMessages(CancellationToken cancellationToken)
    {
        KernelFunction function = _kernel.CreateFunctionFromPrompt(PromptTemplate, ExecutionSettings);
        FunctionResult result = await _kernel.InvokeAsync(function, KernelArguments, cancellationToken);

        string[] choices = result.ToString().Split(Separator,
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (choices.Length < _options.AI.ChoicesCount)
        {
            _logger.LogWarning("Received {ActualChoices} options, expected {ExpectedChoices}. Ignoring the last option.",
                choices.Length, _options.AI.ChoicesCount);

            return choices.Take(choices.Length - 1);
        }

        return choices;
    }
}
