using CraftyNative.ThreeD;
using System.Diagnostics;

namespace CraftyNative.Scenes;

public sealed class GameWorld : IDisposable
{
    public Scene Scene;

    public GameWorld(Window window)
    {
        CraftyNative3D.Initialize(window.NativeWindow);
    }

    private long worstRenderTime;

    public void Render(double deltaTime)
    {
        foreach (var system in Scene.Systems)
            system.Update(ref Scene, deltaTime);

        WorldObject camera = Scene.GetEntitiesWith<Camera>().FirstOrDefault();

        var stopWatch = Stopwatch.StartNew();
        CraftyNative3D.Render(ref Scene, camera);
        stopWatch.Stop();

        long renderTime = stopWatch.ElapsedMilliseconds;
        if (worstRenderTime < renderTime)
        {
            Console.WriteLine($"Worst Render Time: {renderTime}ms");
            worstRenderTime = renderTime;
        }
    }

    public void Dispose()
    {
        CraftyNative3D.Dispose();
    }
}
