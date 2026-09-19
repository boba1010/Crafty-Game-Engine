using Silk.NET.Windowing;
using System.Runtime.InteropServices;

namespace CraftyNative.ThreeD.Scenes;

public sealed class Scene : IDisposable
{
    public Camera Camera { get; }
    private uint _nextObjectId;
    private readonly Dictionary<uint, Transform> _transforms = [];
    private readonly Dictionary<uint, Renderable> _renderables = [];
    internal IReadOnlyDictionary<uint, Renderable> Renderables => _renderables;

    public Scene(IWindow window)
    {
        Camera = new(window)
        {
            Transform = new() { Position = new(0, 0, -3) }
        };
    }

    public WorldObject CreateObject()
    {
        var objectId = new WorldObject(_nextObjectId++);
        _transforms[objectId.Id] = new Transform();
        return objectId;
    }

    public void SetRenderable(WorldObject obj, Renderable renderable)
    {
        _renderables[obj.Id] = renderable;
    }

    public ref Transform GetTransform(WorldObject worldObject)
    {
        return ref CollectionsMarshal.GetValueRefOrNullRef(_transforms, worldObject.Id);
    }

    public ref Renderable GetRenderable(WorldObject worldObject)
    {
        return ref CollectionsMarshal.GetValueRefOrNullRef(_renderables, worldObject.Id);
    }

    public void Render()
    {
        CraftyNative3D.Render(this);
    }

    public void Dispose()
    {
        Camera.Dispose();
    }
}
