using CraftyNative.ThreeD.World;
using System.Collections.Concurrent;
using System.Numerics;

namespace CraftyNative.ThreeD.Meshes;

public static class WorldMeshManager
{
    private static readonly ConcurrentDictionary<SectionCoordinate, WorldMeshSection> _sections = new();

    public static float RenderDistance { get; set; } = 256f;
    public static ICollection<WorldMeshSection> Sections => _sections.Values;

    public static bool TryAdd(WorldMeshSection section)
    {
        return _sections.TryAdd(section.Coordinate, section);
    }

    public static bool RemoveSection(SectionCoordinate coordinate)
    {
        return _sections.Remove(coordinate, out _);
    }

    public static bool RemoveChunk(int x, int z)
    {
        bool removed = false;

        foreach (var coordinate in _sections.Keys)
        {
            if (coordinate.ChunkX != x || coordinate.ChunkZ != z)
                continue;

            removed |= _sections.TryRemove(coordinate, out _);
        }

        return removed;
    }

    public static bool TryGet(SectionCoordinate coordinate, out WorldMeshSection? section)
    {
        return _sections.TryGetValue(coordinate, out section);
    }

    public static IEnumerable<WorldMeshSection> GetVisibleSections(Vector3 cameraPosition)
    {
        float renderDistanceSquared = RenderDistance * RenderDistance;

        foreach (var section in _sections.Values)
        {
            Vector3 delta = section.Bounds.Center - cameraPosition;

            if (delta.LengthSquared() > renderDistanceSquared)
                continue;

            yield return section;
        }
    }

    public static void Clear()
    {
        _sections.Clear();
    }
}
