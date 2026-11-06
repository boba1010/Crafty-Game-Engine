using CraftyNative.ECS;

namespace CraftyNative.Scenes;

public sealed class GameWorld : IDisposable
{
    public Scene Scene;

    public GameWorld(Window window)
    {
        Rendering.CraftyNative.Initialize(window.NativeWindow);
    }

    public void Render(double deltaTime)
    {
        foreach (var system in Scene.Systems)
        {
            if (GameStateManager.IsPaused && system is IGameplaySystem)
                continue;

            system.Update(ref Scene, deltaTime);
        }

        WorldObject camera = Scene.GetEntitiesWith<Camera>().FirstOrDefault();

        Rendering.CraftyNative.Render(ref Scene, camera, deltaTime);
    }

    public void Dispose()
    {
        Rendering.CraftyNative.Dispose();
    }
}
