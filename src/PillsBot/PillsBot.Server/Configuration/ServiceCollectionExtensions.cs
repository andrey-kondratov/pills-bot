using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using PillsBot.Server.Chat;
using PillsBot.Server.Persistence;
using PillsBot.Server.TextGeneration;

namespace PillsBot.Server.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPillsBot(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection configurationSection = configuration.GetSection("PillsBot");
            PillsBotOptions options = new();
            configurationSection.Bind(options);

            services
                .AddOptions()
                .Configure<PillsBotOptions>(configurationSection);

            services
                .AddTransient<IChatClient, TelegramChatClient>()
                .AddHostedService<BotService>();

            if (!options.AI.Enabled)
            {
                services.AddTransient<IMessageProvider, ConfigurationMessageProvider>();

                return services;
            }

            if (options.AI.Azure is null)
            {
                throw new InvalidOperationException("Missing Azure AI configuration.");
            }

            services
                .AddTransient<ConfigurationMessageProvider>()
                .AddScoped<IMessageProvider, AzureOpenAIMessageProvider>()
                .AddKernel()
                .AddAzureOpenAIChatCompletion(options.AI.Azure.DeploymentName, options.AI.Azure.Endpoint, options.AI.Azure.Key);

            services
                .AddDbContext<PillsBotDbContext>(options => options
                    .UseNpgsql(configuration.GetConnectionString("PillsBotDbContext"))
                    .UseSnakeCaseNamingConvention())
                .AddScoped<IMessagesRepository, MessagesRepository>();

            return services;
        }
    }
}
