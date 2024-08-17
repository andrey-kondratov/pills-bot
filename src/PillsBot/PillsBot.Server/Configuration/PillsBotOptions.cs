namespace PillsBot.Server.Configuration
{
    public class PillsBotOptions
    {
        public TelegramOptions? Telegram { get; set; }
        public ReminderOptions Reminder { get; set; } = new();
        public AIOptions AI { get; set; } = new();
    }
}
