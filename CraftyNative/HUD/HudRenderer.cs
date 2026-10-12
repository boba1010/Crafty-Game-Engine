using System.Numerics;
using Vulcan;
using Vulcan.Graphics;

namespace CraftyNative.HUD;

public static class HudRenderer
{
    public static void Initialize(IGraphicsDevice device, Vector2 size)
    {
        Crosshair.Initialize(device, size);
    }

    public static void Render(ICommandBuffer commandBuffer, Vector2 size)
    {
        Crosshair.Render(commandBuffer);
    }

    public static void Dispose()
    {
        Crosshair.Dispose();
    }
}