namespace PillsBot.Server.Configuration
{
    public class PillsBotOptions
    {
        public ConnectionOptions Connection { get; set; }
    }

    public class ConnectionOptions
    {
        public string ApiToken { get; set; }
        public int ChatId { get; set; }
    }
}
