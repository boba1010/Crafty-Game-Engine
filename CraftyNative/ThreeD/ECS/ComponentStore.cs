using System.Runtime.InteropServices;

namespace CraftyNative.ThreeD.ECS;

public sealed class ComponentStore<T> where T : struct, IComponent
{
    private readonly Dictionary<uint, T> _components = [];
    private readonly List<uint> _entities = [];

    public void Add(uint entityId, T component)
    {
        if (!_components.ContainsKey(entityId))
            _entities.Add(entityId);
        _components[entityId] = component;
    }

    public bool Contains(uint entityId)
    {
        return _components.ContainsKey(entityId);
    }

    public ref T Get(uint entityId)
    {
        return ref CollectionsMarshal.GetValueRefOrNullRef(_components, entityId);
    }

    public IEnumerable<uint> Entities => _entities;
}
