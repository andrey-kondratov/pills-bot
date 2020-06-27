using System;

namespace PillsBot.Server.Configuration
{
    public class PillsBotOptions
    {
        public ConnectionOptions Connection { get; set; }
        public ReminderOptions Reminder { get; set; } = new ReminderOptions();
    }

    public class ConnectionOptions
    {
        public string ApiToken { get; set; }
        public long? ChatId { get; set; }
    }

    public class ReminderOptions
    {
        public DateTime Begins { get; set; } = new DateTime(2020, 1, 1, 8, 00, 00, DateTimeKind.Utc);
        public TimeSpan Interval { get; set; } = TimeSpan.FromDays(.5);
        public string Message { get; set; } = "Pills time!";
    }
}
