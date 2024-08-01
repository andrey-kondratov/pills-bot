using System;
using Microsoft.Extensions.Logging;

namespace PillsBot.Server.Configuration
{
    public class PillsBotOptions
    {
        public ConnectionOptions Connection { get; set; } = new();
        public ReminderOptions Reminder { get; set; } = new();
        public AIOptions AI { get; set; } = new();
    }

    public class ConnectionOptions
    {
        public string ApiToken { get; set; }
        public long? ChatId { get; set; }
    }

    public class ReminderOptions
    {
        public DateTime Begins { get; set; } = DateTime.Now.AddSeconds(5);
        public TimeSpan Interval { get; set; } = TimeSpan.FromDays(.5);
        public string Message { get; set; } = "Pills time!";
    }

    public class AIOptions
    {
        public bool Enabled { get; set; } = false;
        public string Languages { get; set; } = "English";
        public string PetNames { get; set; } = "unknown";
        public string PetGender { get; set; } = "unknown";
        public LogLevel LogLevel { get; set; } = LogLevel.Warning;
        public AzureOpenAIOptions Azure { get; set; } = new();

        public class AzureOpenAIOptions
        {
            public string Endpoint { get; set; }
            public string Key { get; set; }
            public string DeploymentName { get; set; }
        }
    }
}
