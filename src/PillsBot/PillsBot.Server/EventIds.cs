using Microsoft.Extensions.Logging;

namespace PillsBot.Server
{
    public static class EventIds
    {
        public static readonly EventId TelegramClientCreationFailed = new(1, nameof(TelegramClientCreationFailed));
        internal static readonly EventId BotStartupFailed = new(2, nameof(BotStartupFailed));
        internal static readonly EventId BotShutdownFailed = new(3, nameof(BotShutdownFailed));
    }
}
