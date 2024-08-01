using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
                .AddTransient<AzureOpenAIMessageProvider>()
                .AddTransient<ConfigurationMessageProvider>()
                .AddTransient<IMessageProvider>(provider => provider
                    .GetRequiredService<IOptions<PillsBotOptions>>().Value.AI.Enabled
                        ? provider.GetRequiredService<AzureOpenAIMessageProvider>()
                        : provider.GetRequiredService<ConfigurationMessageProvider>());

            return services;
        }
    }
}
