using System;

namespace PillsBot.Server;

internal static class TypeExtensions
{
    public static string GetAssemblyVersionString(this Type type) => type.Assembly.GetName()?.Version?.ToString(3) ?? string.Empty;
}
