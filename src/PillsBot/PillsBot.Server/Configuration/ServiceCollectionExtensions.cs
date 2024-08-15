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
                .AddTransient<ITelegramClientFactory, TelegramClientFactory>()
                .AddTransient<IMessenger, TelegramMessenger>()
                .AddHostedService<BotService>();

            services
                .AddSingleton<IChatCompletionService>(provider =>
                {
                    AIOptions.AzureOpenAIOptions options = provider.GetRequiredService<IOptions<PillsBotOptions>>().Value.AI.Azure;

                    return new AzureOpenAIChatCompletionService(options.DeploymentName, options.Endpoint, options.Key, 
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
