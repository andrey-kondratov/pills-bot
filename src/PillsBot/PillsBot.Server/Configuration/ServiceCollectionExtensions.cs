using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using PillsBot.Server.Chat;
using PillsBot.Server.TextGeneration;

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
                .AddTransient<IChatClient, TelegramChatClient>()
                .AddHostedService<BotService>();

            AIOptions aiOptions = new();
            configuration.GetSection("AI").Bind(aiOptions);

            if (!aiOptions.Enabled)
            {
                services.AddTransient<IMessageProvider, ConfigurationMessageProvider>();

                return services;
            }

            if (aiOptions.Azure is null)
            {
                throw new InvalidOperationException("Missing Azure AI configuration.");
            }

            services
                .AddTransient<ConfigurationMessageProvider>()
                .AddSingleton<IMessageProvider, AzureOpenAIMessageProvider>()
                .AddKernel()
                .AddAzureOpenAIChatCompletion(aiOptions.Azure.DeploymentName, aiOptions.Azure.Endpoint, aiOptions.Azure.Key);

            return services;
        }
    }
}
