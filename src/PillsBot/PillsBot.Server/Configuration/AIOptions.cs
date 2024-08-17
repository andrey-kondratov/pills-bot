using Microsoft.Extensions.Logging;

namespace PillsBot.Server.Configuration
{
    public class AIOptions
    {
        public bool Enabled { get; set; } = false;
        public string Languages { get; set; } = "English";
        public string PetNames { get; set; } = "unknown";
        public string PetGender { get; set; } = "unknown";
        public LogLevel LogLevel { get; set; } = LogLevel.Warning;
        public AzureOpenAIOptions? Azure { get; set; } = null;
        public int ChoicesCount { get; set; } = 20;
        public int MaxTokens { get; set; } = 1000;

        public class AzureOpenAIOptions
        {
            public required string Endpoint { get; set; }
            public required string Key { get; set; }
            public required string DeploymentName { get; set; }
        }
    }
}
