using Silk.NET.Windowing;

namespace CraftyNative.ThreeD.Scenes;

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

        WorldObject camera = new();

        foreach (var cam in Scene.GetEntitiesWith<Camera>())
            camera = cam;

        CraftyNative3D.Render(ref Scene, camera);
    }

    public void Dispose()
    {
        CraftyNative3D.Dispose();
    }
}
