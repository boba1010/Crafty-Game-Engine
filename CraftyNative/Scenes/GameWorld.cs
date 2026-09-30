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

    public void Render(double deltaTime)
    {
        foreach (var system in Scene.Systems)
            system.Update(ref Scene, deltaTime);

        WorldObject camera = Scene.GetEntitiesWith<Camera>().FirstOrDefault();

        CraftyNative3D.Render(ref Scene, camera);
    }

    public void Dispose()
    {
        CraftyNative3D.Dispose();
    }
}
