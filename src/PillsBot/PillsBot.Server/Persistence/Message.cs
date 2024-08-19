using System;

namespace PillsBot.Server.Persistence;

public sealed class Message
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
    public required string Language { get; set; }
    public required string Reminder { get; set; }
    public required string Acknowledgement { get; set; }
    public required string Appreciation { get; set; }
}
