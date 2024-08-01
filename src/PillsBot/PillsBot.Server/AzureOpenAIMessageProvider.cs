using System;
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
    private readonly IOptions<PillsBotOptions> _options;
    private readonly ILogger<AzureOpenAIMessageProvider> _logger;
    private readonly ConfigurationMessageProvider _configurationMessageProvider;
    private readonly Kernel _kernel;

    public AzureOpenAIMessageProvider(IOptions<PillsBotOptions> options, 
        ILogger<AzureOpenAIMessageProvider> logger,
        ConfigurationMessageProvider configurationMessageProvider)
    {
        _options = options;
        _logger = logger;
        _configurationMessageProvider = configurationMessageProvider;

        string endpoint = options.Value.AI.Azure.Endpoint
            ?? throw new InvalidOperationException("Azure OpenAI endpoint is not configured.");
        string key = options.Value.AI.Azure.Key
            ?? throw new InvalidOperationException("Azure OpenAI key is not configured.");
        string deploymentName = options.Value.AI.Azure.DeploymentName
            ?? throw new InvalidOperationException("Azure OpenAI deployment name is not configured.");
            
        IKernelBuilder builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(deploymentName, endpoint, key);

        builder.Services.AddLogging(builder => 
            builder.AddConsole().SetMinimumLevel(options.Value.AI.LogLevel));

        _kernel = builder.Build();
    }

    public Task<string> GetMessage(CancellationToken cancellationToken = default)
    {
        string systemPrompt = @"
            You are a friendly assistant who reminds cat owners
            to give a pill to their cat twice a day.";

        string promptTemplate = @"
            The cat's names are {{$names}}. You may include a single name in the message, but be sure to address the owners, not the cat.
            The cat's gender is {{$gender}}. Make sure to consider the gender if the language has different words for the cat depending on the gender.

            Generate a short message reminding the owners that it is now time to give a pill. 
            The message should be addressed to a human who will be giving the cat a pill.

            The message must end with an exclamation sign.

            Keep the message length at most 3 words.
            Use either of these languages with uniform probability: {{$languages}}. 
            ";

        OpenAIPromptExecutionSettings executionSettings = new()
        {
            ChatSystemPrompt = systemPrompt,
            Temperature = 0.7,
            MaxTokens = 100
        };

        KernelArguments args = new(executionSettings)
        {
            ["languages"] = _options.Value.AI.Languages,
            ["names"] = _options.Value.AI.PetNames,
            ["gender"] = _options.Value.AI.PetGender
        };

        KernelFunction function = _kernel.CreateFunctionFromPrompt(promptTemplate, executionSettings);

        return GetMessageInternal(function, args, cancellationToken);
    }

    private async Task<string> GetMessageInternal(KernelFunction function, KernelArguments arguments, 
        CancellationToken cancellationToken)
    {
        try
        {
            FunctionResult result = await _kernel.InvokeAsync(function, arguments, cancellationToken);
            return result.ToString();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error invoking the prompt. Defaulting to the message in configuration.");
            return await _configurationMessageProvider.GetMessage(cancellationToken);
        }
    }
}
