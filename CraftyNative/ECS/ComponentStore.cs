using System.Runtime.InteropServices;

namespace CraftyNative.ECS;

public sealed class ComponentStore<T> where T : struct, IComponent
{
    private readonly Dictionary<uint, T> _components = [];
    private readonly List<uint> _entities = [];
    private readonly Dictionary<uint, int> _entityIndices = [];

    public void Add(uint entityId, T component)
    {
        if (!_components.ContainsKey(entityId))
        {
            _entityIndices[entityId] = _entities.Count;
            _entities.Add(entityId);
        }
        _components[entityId] = component;
    }

    public void Remove(uint entityId)
    {
        if (!_components.Remove(entityId))
            return;

        int index = _entityIndices[entityId];
        int lastIndex = _entities.Count - 1;
        uint lastEntity = _entities[lastIndex];

        _entities[index] = lastEntity;
        _entityIndices[lastEntity] = index;

        _entities.RemoveAt(lastIndex);
        _entityIndices.Remove(entityId);
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
