using CraftyNative.ECS;
using CraftyNative.ThreeD;
using System.Runtime.InteropServices;

namespace CraftyNative.Scenes;

public struct Scene
{
    private uint _nextObjectId;
    private readonly Dictionary<uint, Renderable> _renderables = [];
    private readonly Dictionary<uint, uint> _parents = [];
    public List<ISystem> Systems { get; } = [];
    private readonly Dictionary<Type, object> _componentStores = [];
    internal readonly IReadOnlyDictionary<uint, Renderable> Renderables => _renderables;

    public Scene()
    {
        Systems.Add(new HierarchySystem());
    }

    public void SetParent(WorldObject child, WorldObject parent)
    {
        _parents[child.Id] = parent.Id;
    }

    public WorldObject? GetParent(WorldObject child)
    {
        if (!_parents.TryGetValue(child.Id, out var parentId))
            return null;

        return new WorldObject(parentId);
    }

    public WorldObject[] GetChildren(WorldObject parent)
    {
        var children = new List<WorldObject>();

        foreach (var (childId, parentId) in _parents)
        {
            if (parentId == parent.Id)
                children.Add(new WorldObject(childId));
        }

        return children.ToArray();
    }

    public WorldObject CreateObject()
    {
        return new WorldObject(_nextObjectId++);
    }

    public void SetRenderable(WorldObject obj, Renderable renderable)
    {
        _renderables[obj.Id] = renderable;
    }

    public ref Renderable GetRenderable(WorldObject worldObject)
    {
        return ref CollectionsMarshal.GetValueRefOrNullRef(_renderables, worldObject.Id);
    }

    public void RemoveRenderable(WorldObject obj)
    {
        _renderables.Remove(obj.Id);
    }

    private ComponentStore<T> GetStore<T>() where T : struct, IComponent
    {
        if (!_componentStores.TryGetValue(typeof(T), out var value))
        {
            var store = new ComponentStore<T>();
            _componentStores.Add(typeof(T), store);
            return store;
        }

        return (ComponentStore<T>)value;
    }

    public void AddComponent<T>(WorldObject obj, T component) where T : unmanaged, IComponent
    {
        GetStore<T>().Add(obj.Id, component);
    }

    public ref T GetComponent<T>(WorldObject obj) where T : unmanaged, IComponent
    {
        return ref GetStore<T>().Get(obj.Id);
    }

    public IEnumerable<WorldObject> GetEntitiesWith<T>() where T : struct, IComponent
    {
        foreach (var id in GetStore<T>().Entities)
            yield return new WorldObject(id);
    }

    public bool HasComponent<T>(WorldObject obj) where T : unmanaged, IComponent
    {
        return GetStore<T>().Contains(obj.Id);
    }
}
