using System;

namespace PillsBot.Server.Configuration
{
    public class ReminderOptions
    {
        public DateTime Begins { get; set; } = DateTime.Now.AddSeconds(5);
        public TimeSpan Interval { get; set; } = TimeSpan.FromDays(.5);
        public string Message { get; set; } = "Pills time!";
    }
}
