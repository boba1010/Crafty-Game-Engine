using CraftyNative.Scenes;
using CraftyNative.ThreeD;

namespace CraftyNative.ViewModels;

public sealed class ViewModel
{
    public WorldObject Object { get; }

    public ViewModel(Scene scene)
    {
        Object = scene.CreateObject();

        scene.AddComponent(Object, new Transform
        {
            LocalPosition = new(0.35f, -0.35f, 0.65f)
        });
    }
}