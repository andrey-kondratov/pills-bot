using System;

namespace PillsBot.Server.TextGeneration;

public sealed class ChatCompletionException(Exception? innerException) : Exception("Error getting the chat message content.", innerException)
{
}
