using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            return services;
        }
    }
}
