using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Serilog;

namespace PillsBot.Server.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPillsBot(this IServiceCollection services, IConfigurationSection configuration)
        {
            services
                .AddOptions()
                .Configure<PillsBotOptions>(configuration);

            services
                .AddTransient<IMessenger, TelegramMessenger>()
                .AddHostedService<BotService>();

            services
                .AddSingleton<IChatCompletionService>(provider =>
                {
                    AIOptions options = provider.GetRequiredService<IOptions<PillsBotOptions>>().Value.AI;

                    if (!options.Enabled)
                    {
                        throw new InvalidOperationException("AI features are disabled.");
                    }

                    if (options.Azure is null)
                    {
                        throw new InvalidOperationException("Missing Azure AI configuration.");
                    }

                    return new AzureOpenAIChatCompletionService(options.Azure.DeploymentName, options.Azure.Endpoint, options.Azure.Key, 
                        loggerFactory: new LoggerFactory()
                            .AddSerilog(provider.GetRequiredService<Serilog.ILogger>()));
                })
                .AddSingleton<AzureOpenAIMessageProvider>()
                .AddTransient<ConfigurationMessageProvider>()
                .AddTransient<IMessageProvider>(provider => provider
                    .GetRequiredService<IOptions<PillsBotOptions>>().Value.AI.Enabled
                        ? provider.GetRequiredService<AzureOpenAIMessageProvider>()
                        : provider.GetRequiredService<ConfigurationMessageProvider>());

            return services;
        }
    }
}
