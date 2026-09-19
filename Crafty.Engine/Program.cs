using Crafty.Engine.UI;

namespace Crafty.Engine;

internal class Program
{
    public static MainWindow Window { get; private set; } = null!;

    public static void Main()
    {
        try
        {
            Window = new()
            {
                Title = "Crafty"
            };

            Window.Activate();
        }
        finally
        {
            Dispose();
        }
    }

    private static void Dispose()
    {
        Window?.Dispose();
    }
}