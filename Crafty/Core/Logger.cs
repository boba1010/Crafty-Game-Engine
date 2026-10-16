using Crafty.SDK.Debugging;

namespace Crafty.Core;

public sealed class Logger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine(message);
    }
}
