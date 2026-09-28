using Crafty.SDK.Debugging;

namespace Crafty.Engine;

public sealed class Logger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine(message);
    }
}
